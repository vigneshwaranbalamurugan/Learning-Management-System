#!/bin/bash

# Ensure script fails on any error
set -e

# Colors for pretty output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 LMS Automated Setup Starting...${NC}\n"

# ─── 1. Pre-flight Checks ──────────────────────────────────────────────────────
echo -e "${YELLOW}Checking prerequisites...${NC}"

if ! command -v az &> /dev/null; then
    echo -e "${RED}Azure CLI (az) could not be found. Please install it (brew install azure-cli).${NC}"
    exit 1
fi

if ! command -v gh &> /dev/null; then
    echo -e "${RED}GitHub CLI (gh) could not be found. Please install it (brew install gh).${NC}"
    exit 1
fi

if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}kubectl could not be found. Please install it (brew install kubectl).${NC}"
    exit 1
fi

if ! command -v jq &> /dev/null; then
    echo -e "${RED}jq could not be found. Please install it (brew install jq).${NC}"
    exit 1
fi

# Check if logged in to Azure
if ! az account show &> /dev/null; then
    echo -e "${RED}You are not logged into Azure. Please run 'az login' first.${NC}"
    exit 1
fi

# Check if logged in to GitHub CLI
if ! gh auth status &> /dev/null; then
    echo -e "${RED}You are not logged into GitHub CLI. Please run 'gh auth login' first.${NC}"
    exit 1
fi

# ─── 2. Load Environment Variables ─────────────────────────────────────────────
echo -e "${YELLOW}Loading environment variables...${NC}"
if [ ! -f .env ]; then
    echo -e "${RED}No .env file found! Copying .env.template to .env...${NC}"
    cp .env.template .env
    echo -e "${RED}Please fill out all the secrets in the .env file and run this script again!${NC}"
    exit 1
fi

source .env

# Validate required variables
REQUIRED_VARS=("POSTGRES_PASSWORD" "JWT_KEY" "DOCKERHUB_USERNAME" "DOCKERHUB_TOKEN")
for VAR in "${REQUIRED_VARS[@]}"; do
    if [ -z "${!VAR}" ]; then
        echo -e "${RED}Missing required environment variable: $VAR in .env file!${NC}"
        exit 1
    fi
done

# ─── 3. Deploy Azure Infrastructure ────────────────────────────────────────────
RG="coursehublms-rg-dev"
LOCATION="centralindia"

echo -e "\n${YELLOW}Creating Resource Group: $RG...${NC}"
az group create --name "$RG" --location "$LOCATION" -o none

echo -e "${YELLOW}Cleaning up any soft-deleted Key Vaults from previous failed runs...${NC}"
DELETED_KV=$(az keyvault list-deleted --query "[?contains(name, 'lmskv')].name" -o tsv)
if [ ! -z "$DELETED_KV" ]; then
    while IFS= read -r kv; do
        echo -e "${YELLOW}Purging soft-deleted Key Vault: $kv...${NC}"
        # Some regions might take a moment to fully purge
        az keyvault purge --name "$kv" || true
    done <<< "$DELETED_KV"
fi

echo -e "${YELLOW}Deploying Bicep Templates (This takes ~15 minutes)...${NC}"
BICEP_OUTPUT=$(az deployment group create \
  --resource-group "$RG" \
  --template-file azure/main.bicep \
  --parameters postgresPassword="$POSTGRES_PASSWORD" \
  --query properties.outputs \
  --output json)

echo -e "${GREEN}Bicep Deployment Successful!${NC}"

# Parse Bicep Outputs
KV_NAME=$(echo "$BICEP_OUTPUT" | jq -r '.keyVaultName.value')
UAMI_CLIENT_ID=$(echo "$BICEP_OUTPUT" | jq -r '.uamiClientId.value')
POSTGRES_HOST=$(echo "$BICEP_OUTPUT" | jq -r '.postgresHost.value')
REDIS_HOST=$(echo "$BICEP_OUTPUT" | jq -r '.redisHost.value')
STORAGE_NAME=$(echo "$BICEP_OUTPUT" | jq -r '.storageAccountName.value')
SWA_TOKEN=$(echo "$BICEP_OUTPUT" | jq -r '.swaDeploymentToken.value')
SWA_HOSTNAME=$(echo "$BICEP_OUTPUT" | jq -r '.staticWebAppHostname.value')
TENANT_ID=$(az account show --query tenantId -o tsv)

# ─── 4. Configure Key Vault ────────────────────────────────────────────────────
echo -e "\n${YELLOW}Configuring Key Vault Access for your account...${NC}"
USER_OID=$(az ad signed-in-user show --query id -o tsv)
az keyvault set-policy --name "$KV_NAME" --object-id "$USER_OID" --secret-permissions get list set delete -o none

echo -e "${YELLOW}Populating Key Vault Secrets...${NC}"

# Helper function
set_secret() {
    az keyvault secret set --vault-name "$KV_NAME" --name "$1" --value "$2" -o none
    echo "  -> Set secret $1"
}

set_secret "Cloudinary--CloudName" "$CLOUDINARY_CLOUD_NAME"
set_secret "Cloudinary--ApiKey" "$CLOUDINARY_API_KEY"
set_secret "Cloudinary--ApiSecret" "$CLOUDINARY_API_SECRET"

set_secret "Smtp--Username" "$SMTP_USERNAME"
set_secret "Smtp--Password" "$SMTP_PASSWORD"

set_secret "Jwt--Key" "$JWT_KEY"

set_secret "ConnectionStrings--DefaultConnection" "Host=${POSTGRES_HOST};Port=5432;Database=coursehubdb;Username=postgresadmin;Password=${POSTGRES_PASSWORD};SSL Mode=Require"

set_secret "Razorpay--KeyId" "$RAZORPAY_KEY_ID"
set_secret "Razorpay--KeySecret" "$RAZORPAY_KEY_SECRET"
set_secret "Razorpay--WebhookSecret" "$RAZORPAY_WEBHOOK_SECRET"

set_secret "Stripe--SecretKey" "$STRIPE_SECRET_KEY"

# Blob Storage Connection String
BLOB_KEY=$(az storage account keys list --account-name "$STORAGE_NAME" --resource-group "$RG" --query "[0].value" -o tsv)
set_secret "AzureBlob--ConnectionString" "DefaultEndpointsProtocol=https;AccountName=${STORAGE_NAME};AccountKey=${BLOB_KEY};EndpointSuffix=core.windows.net"

# Redis Connection String
REDIS_NAME=$(az redis list --resource-group "$RG" --query '[0].name' -o tsv)
REDIS_KEY=$(az redis list-keys --resource-group "$RG" --name "$REDIS_NAME" --query primaryKey -o tsv)
set_secret "Redis--ConnectionString" "${REDIS_HOST}:6380,password=${REDIS_KEY},ssl=True,abortConnect=False"

echo -e "${GREEN}Successfully populated 13 Key Vault secrets!${NC}"

# ─── 5. Configure AKS and GitHub Secrets ───────────────────────────────────────
echo -e "\n${YELLOW}Getting AKS Credentials...${NC}"
# Isolate the config to prevent uploading stale clusters from ~/.kube/config
KUBECONFIG=~/.kube/clean_config az aks get-credentials --resource-group "$RG" --name lms-aks-dev --overwrite-existing
KUBE_CONFIG_B64=$(cat ~/.kube/clean_config | base64 | tr -d '\n')

# Also update the user's local config for their own terminal
az aks get-credentials --resource-group "$RG" --name lms-aks-dev --overwrite-existing

echo -e "${YELLOW}Updating GitHub Secrets (Repo level)...${NC}"
gh secret set KUBE_CONFIG_B64 -b "$KUBE_CONFIG_B64"
gh secret set AZURE_TENANT_ID -b "$TENANT_ID"
gh secret set UAMI_CLIENT_ID -b "$UAMI_CLIENT_ID"
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN -b "$SWA_TOKEN"
gh secret set DOCKERHUB_USERNAME -b "$DOCKERHUB_USERNAME"
gh secret set DOCKERHUB_TOKEN -b "$DOCKERHUB_TOKEN"
gh secret set RAZORPAYKEY -b "$RAZORPAY_KEY_ID"
gh secret set KEY_VAULT_NAME -b "$KV_NAME"

echo -e "${YELLOW}Updating GitHub Secrets (Development Environment level)...${NC}"
gh secret set KUBE_CONFIG_B64 -b "$KUBE_CONFIG_B64" -e Development
gh secret set AZURE_TENANT_ID -b "$TENANT_ID" -e Development
gh secret set UAMI_CLIENT_ID -b "$UAMI_CLIENT_ID" -e Development
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN -b "$SWA_TOKEN" -e Development
gh secret set DOCKERHUB_USERNAME -b "$DOCKERHUB_USERNAME" -e Development
gh secret set DOCKERHUB_TOKEN -b "$DOCKERHUB_TOKEN" -e Development
gh secret set RAZORPAYKEY -b "$RAZORPAY_KEY_ID" -e Development
gh secret set KEY_VAULT_NAME -b "$KV_NAME" -e Development

echo -e "${YELLOW}Updating GitHub Variables...${NC}"
gh variable set SWA_HOSTNAME -b "$SWA_HOSTNAME"
gh variable set SWA_HOSTNAME -b "$SWA_HOSTNAME" -e Development

echo -e "\n${GREEN}🎉 Setup Complete! 🎉${NC}"
echo -e "All Azure infrastructure is deployed, Key Vault is populated, and GitHub Secrets are set!"
echo -e "\n${YELLOW}Next Steps:${NC}"
echo -e "1. Go to your GitHub Actions tab"
echo -e "2. Run 'K8s Cluster Setup'"
echo -e "3. Run 'Deploy Backend'"
echo -e "4. Once the backend finishes, run './update_github_urls.sh'"
echo -e "5. Run 'Deploy Frontend'"
