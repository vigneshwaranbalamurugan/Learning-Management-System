# LMS Deployment Setup Guide

> **Permission model:** Azure **Contributor** only. No App Registration creation.
> No role assignments. No Azure SP in GitHub Actions.
>
> Everything Azure-related runs **locally from your machine** using `az login`.
> GitHub Actions only uses Docker Hub credentials and a kubeconfig file —
> both of which you extract locally and store as GitHub Secrets.

---

## How Authentication Works

```
Your Machine (az login — Contributor)
  └── az deployment group create   → deploys all Azure resources (Bicep)
  └── az keyvault secret set       → populates Key Vault secrets
  └── az aks get-credentials       → downloads kubeconfig → base64 → GitHub Secret
  └── helm install                 → installs CSI Driver on AKS

GitHub Actions (NO Azure SP required)
  ├── K8s Setup workflow     → uses KUBE_CONFIG_B64  only
  ├── Backend workflow       → uses DOCKERHUB_TOKEN + KUBE_CONFIG_B64
  └── Frontend workflow      → uses AZURE_STATIC_WEB_APPS_API_TOKEN only
```

---

## Prerequisites

Install on your local machine:
- **Azure CLI**: `brew install azure-cli`
- **kubectl**: `brew install kubectl`
- **helm**: `brew install helm`

---

## Step 1 — Local: Deploy Azure Infrastructure

Open Terminal and run these commands in order.

### 1a. Login and create resource group
```bash
az login
az group create --name coursehublms-rg-dev --location centralindia
```

### 1b. Deploy Bicep (all Azure resources)
```bash
cd /path/to/LMS    # your repo root

az deployment group create \
  --resource-group coursehublms-rg-dev \
  --template-file azure/main.bicep \
  --parameters postgresPassword='<choose-a-strong-password>' \
  --query properties.outputs \
  --output json
```

Save the output — you'll need these values shortly:
- `keyVaultName.value` → save as GitHub Secret **`KEY_VAULT_NAME`**
- `uamiClientId.value` → save as GitHub Secret **`UAMI_CLIENT_ID`**
- `uamiPrincipalId.value` *(printed for reference, already used by Bicep internally)*
- `postgresHost.value`
- `redisHost.value`
- `storageAccountName.value`
- `swaDeploymentToken.value` → save as GitHub Secret **`AZURE_STATIC_WEB_APPS_API_TOKEN`**
- `staticWebAppHostname.value` → save as GitHub Variable **`SWA_HOSTNAME`**


# 1. Get Key Vault Name
az keyvault list --resource-group coursehublms-rg-dev --query '[0].name' -o tsv

# 2. Get User Assigned Managed Identity (UAMI) Client ID & Principal ID
az identity list --resource-group coursehublms-rg-dev --query '[0].clientId' -o tsv
az identity list --resource-group coursehublms-rg-dev --query '[0].principalId' -o tsv

# 3. Get PostgreSQL Hostname
az postgres flexible-server list --resource-group coursehublms-rg-dev --query '[0].fullyQualifiedDomainName' -o tsv

# 4. Get Redis Hostname
az redis list --resource-group coursehublms-rg-dev --query '[0].hostName' -o tsv

# 5. Get Storage Account Name
az storage account list --resource-group coursehublms-rg-dev --query '[0].name' -o tsv

# 6. Get Static Web App Hostname & Deployment Token
az staticwebapp list --resource-group coursehublms-rg-dev --query '[0].defaultHostname' -o tsv
az staticwebapp secrets list --name lms-swa-dev --resource-group coursehublms-rg-dev --query 'properties.apiKey' -o tsv


### 1c. Grant Yourself Key Vault Access
Because we use Key Vault Access Policies (not RBAC), you must grant your own user account permission to write secrets to the newly created vault.

```bash
# Get your Key Vault name
KV="<keyVaultName.value from Step 1b output>"

# Get your signed-in user's Object ID
USER_OID=$(az ad signed-in-user show --query id -o tsv)

# Grant yourself Get, List, Set, and Delete permissions
az keyvault set-policy --name "$KV" --object-id "$USER_OID" --secret-permissions get list set delete
```

### 1d. Populate Key Vault Secrets
```bash
# KV name is auto-generated (globally unique) — get it from the Bicep output above
KV="<keyVaultName.value from Step 1b output>"
RG="coursehublms-rg-dev"
STORAGE_NAME="<storageAccountName from output>"
POSTGRES_HOST="<postgresHost from output>"
REDIS_HOST="<redisHost from output>"

# ── Cloudinary ──────────────────────────────────────────────────────────────
az keyvault secret set --vault-name "$KV" --name "Cloudinary--CloudName"   --value "<your-cloudinary-cloud-name>"
az keyvault secret set --vault-name "$KV" --name "Cloudinary--ApiKey"      --value "<your-cloudinary-api-key>"
az keyvault secret set --vault-name "$KV" --name "Cloudinary--ApiSecret"   --value "<your-cloudinary-api-secret>"

# ── Azure Blob connection string ─────────────────────────────────────────────
BLOB_KEY=$(az storage account keys list \
  --account-name "$STORAGE_NAME" --resource-group "$RG" \
  --query "[0].value" -o tsv)
az keyvault secret set --vault-name "$KV" --name "AzureBlob--ConnectionString" \
  --value "DefaultEndpointsProtocol=https;AccountName=${STORAGE_NAME};AccountKey=${BLOB_KEY};EndpointSuffix=core.windows.net"

# ── SMTP ─────────────────────────────────────────────────────────────────────
az keyvault secret set --vault-name "$KV" --name "Smtp--Username" --value "<gmail-address>"
az keyvault secret set --vault-name "$KV" --name "Smtp--Password" --value "<gmail-app-password>"

# ── JWT signing key ───────────────────────────────────────────────────────────
az keyvault secret set --vault-name "$KV" --name "Jwt--Key" --value "<random-32-char-string>"

# ── PostgreSQL connection string ──────────────────────────────────────────────
az keyvault secret set --vault-name "$KV" --name "ConnectionStrings--DefaultConnection" \
  --value "Host=${POSTGRES_HOST};Port=5432;Database=coursehubdb;Username=postgresadmin;Password=<same-password-as-bicep>;SSL Mode=Require"

# ── Razorpay ─────────────────────────────────────────────────────────────────
az keyvault secret set --vault-name "$KV" --name "Razorpay--KeyId"         --value "<razorpay-key-id>"
az keyvault secret set --vault-name "$KV" --name "Razorpay--KeySecret"     --value "<razorpay-key-secret>"
az keyvault secret set --vault-name "$KV" --name "Razorpay--WebhookSecret" --value "<razorpay-webhook-secret>"

# ── Stripe ────────────────────────────────────────────────────────────────────
az keyvault secret set --vault-name "$KV" --name "Stripe--SecretKey" --value "<stripe-secret-key>"

# ── Redis connection string ───────────────────────────────────────────────────
REDIS_NAME=$(az redis list --resource-group "$RG" --query '[0].name' -o tsv)
REDIS_KEY=$(az redis list-keys \
  --resource-group "$RG" --name "$REDIS_NAME" --query primaryKey -o tsv)
az keyvault secret set --vault-name "$KV" --name "Redis--ConnectionString" \
  --value "${REDIS_HOST}:6380,password=${REDIS_KEY},ssl=True,abortConnect=False"

echo "✅ All 13 Key Vault secrets populated."
```

### 1d. Get kubeconfig → GitHub Secret

```bash
# Download the AKS kubeconfig to your machine
az aks get-credentials \
  --resource-group coursehublms-rg-dev \
  --name lms-aks-dev \
  --overwrite-existing

# Verify it works
kubectl get nodes

# Base64 encode and copy to clipboard (macOS)
cat ~/.kube/config | base64 | pbcopy

# → Paste this as GitHub Secret: KUBE_CONFIG_B64
```

### 1e. Get your Tenant ID → GitHub Secret
```bash
az account show --query tenantId -o tsv
# → Save this as GitHub Secret: AZURE_TENANT_ID
```

---

## Step 2 — Docker Hub Setup

1. Create a free account at **hub.docker.com**
2. Create a repository: `<your-username>/lms-api` (can be private)
3. Generate an Access Token:
   - hub.docker.com → **Account Settings → Security → New Access Token**
   - Name: `lms-github-actions`, Permission: `Read & Write`
   - Copy the token

---

## Step 3 — Add GitHub Secrets & Variables

Repo → **Settings → Secrets and variables → Actions**

### Secrets

| Secret | How to get it |
|---|---|
| `KUBE_CONFIG_B64` | Output of Step 1d |
| `AZURE_TENANT_ID` | Output of Step 1e |
| `UAMI_CLIENT_ID` | `uamiClientId.value` from Bicep output (Step 1b) |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | `swaDeploymentToken.value` from Bicep output (Step 1b) |
| `DOCKERHUB_USERNAME` | Your Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub access token (Step 2) |
| `RAZORPAYKEY` | Razorpay **publishable** key (for Angular frontend) |

> That's it — **no** `AZURE_CREDENTIALS`, no SP, no client secret.

### Variables

| Variable | Value | Timing |
|---|---|---|
| `SWA_HOSTNAME` | `staticWebAppHostname` from Bicep output | After Step 1b |
| `AKS_EXTERNAL_IP` | LoadBalancer IP | After Step 5 (first backend deploy) |
| `APIURL` | `http://<AKS_EXTERNAL_IP>/api/v1` | After Step 5 |
| `HUBURL` | `http://<AKS_EXTERNAL_IP>/hubs` | After Step 5 |
| `HANGFIREURL` | `http://<AKS_EXTERNAL_IP>/hangfire` | After Step 5 |

---

## Step 4 — Run K8s Setup Workflow (GitHub Actions)

1. Go to **Actions → ☸️ K8s Cluster Setup**
2. Click **Run workflow**

This will (using only your KUBE_CONFIG_B64):
- Install Secrets Store CSI Driver
- Install Azure Key Vault Provider
- Create the Docker Hub `imagePullSecret` in namespace `lms`

---

## Step 5 — First Backend Deploy

Push any change to `LMSApi/` on `main`, **or**:
**Actions → 🚀 Deploy Backend (Docker Hub → AKS) → Run workflow**

After it completes:
```bash
kubectl get svc lms-api-svc -n lms
# Wait until EXTERNAL-IP column shows an IP (not <pending>)
```

Copy the IP, then set these GitHub Variables:
```
AKS_EXTERNAL_IP = <the IP>
APIURL          = http://<the IP>/api/v1
HUBURL          = http://<the IP>/hubs
HANGFIREURL     = http://<the IP>/hangfire
```

Re-run the backend workflow once more so pods pick up the correct URLs.

---

## Step 6 — First Frontend Deploy

Push any change to `lms-app/` on `main`, **or**:
**Actions → 🌐 Deploy Frontend (Azure Static Web Apps) → Run workflow**

App is live at `https://<name>.azurestaticapps.net`

---

## Useful Commands

```bash
# Watch pods start up
kubectl get pods -n lms -w

# View API logs
kubectl logs -n lms deployment/lms-api -f

# Confirm secrets are injected
kubectl exec -n lms deployment/lms-api -- env | grep Jwt__Key

# Force pod restart
kubectl delete pod -n lms -l app=lms-api

# Refresh kubeconfig when KUBE_CONFIG_B64 expires
az aks get-credentials --resource-group coursehublms-rg-dev --name lms-aks-dev --overwrite-existing
cat ~/.kube/config | base64 | pbcopy
# Re-save as GitHub Secret: KUBE_CONFIG_B64
```

---

## Architecture

```
YOUR MACHINE (az login — Contributor)
  Step 1a: az group create
  Step 1b: az deployment group create  → deploys VNet, AKS, UAMI, Postgres,
                                          Redis, Key Vault, Blob, SWA
  Step 1c: az keyvault secret set ×13  → populates all secrets
  Step 1d: az aks get-credentials      → kubeconfig → KUBE_CONFIG_B64

GITHUB ACTIONS (no Azure SP at all)
  ☸️ K8s Setup    → kubectl (KUBE_CONFIG_B64) → installs CSI Driver + imagePullSecret
  🚀 Backend      → docker push (Docker Hub) + kubectl (KUBE_CONFIG_B64)
  🌐 Frontend     → SWA deploy (AZURE_STATIC_WEB_APPS_API_TOKEN)

AZURE RESOURCES (coursehublms-rg-dev, centralindia)
  lms-vnet          VNet 10.0.0.0/16
  lms-aks-dev       AKS (1× Standard_B2s, Workload Identity enabled)
  lms-postgres-dev  PostgreSQL 16 (private, VNet-only)
  lms-redis-dev     Redis Basic C0
  lms-kv-dev        Key Vault (access policies, 13 secrets)
  lmsblobstorage*   Blob Storage (lms-media + lms-public)
  lms-swa-dev       Azure Static Web Apps (Free)

DOCKER HUB
  <username>/lms-api:<sha-tag>  ← built by GitHub Actions, pulled by AKS
```

---

## Full GitHub Secrets List (Only 7)

| Secret | Source |
|---|---|
| `KUBE_CONFIG_B64` | Local: `cat ~/.kube/config \| base64` |
| `AZURE_TENANT_ID` | Local: `az account show --query tenantId -o tsv` |
| `UAMI_CLIENT_ID` | Bicep output: `uamiClientId.value` |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Bicep output: `swaDeploymentToken.value` |
| `DOCKERHUB_USERNAME` | Your Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub Access Token |
| `RAZORPAYKEY` | Razorpay dashboard (publishable key) |