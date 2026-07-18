# LMS Deployment Setup Guide

> **Permission model:** Azure **Contributor** only. No App Registration creation.
> No role assignments. No Azure SP in GitHub Actions.
>
> Everything Azure-related runs **locally from your machine** using `az login`.
> We use a master script to completely automate resource provisioning, secrets injection, and GitHub configuration!

---

## Prerequisites

Install on your local machine:
- **Azure CLI**: `brew install azure-cli`
- **GitHub CLI**: `brew install gh`
- **kubectl**: `brew install kubectl`
- **helm**: `brew install helm`
- **jq**: `brew install jq`

**Authenticate to both CLIs:**
```bash
az login
gh auth login
```

---

## Step 1 — Automated Infrastructure Setup

1. Copy the `.env.template` file to `.env`:
   ```bash
   cp .env.template .env
   ```
2. Open `.env` and fill in all your secrets (Postgres password, Cloudinary keys, Docker Hub token, etc.).
3. Run the automated setup script:
   ```bash
   ./auto_setup.sh
   ```

*The script takes around 15 minutes. It will deploy all Azure resources, grant you Key Vault access, securely populate all 13 secrets, grab your cluster credentials, and automatically update all required secrets and variables in your GitHub repository!*

---

## Step 2 — Deploy the Applications (GitHub Actions)

Go to your repository **Actions** tab in GitHub and run the following workflows in order:

### 1. ☸️ K8s Cluster Setup
- **What it does:** Installs Secrets Store CSI Driver (with Workload Identity), NGINX Ingress Controller, and Cert-Manager.
- **Wait for it to finish.**

### 2. 🔑 Inject AI Engine Secrets to Key Vault (One-time, manual)
Because you didn't want the AI API keys sitting in your `.env` file, you must run this quick script in your terminal to securely push them to your Key Vault before deploying the AI Engine.

```bash
# 1. Dynamically get your Key Vault name
KV_NAME=$(az deployment group show -g coursehublms-rg-dev -n main --query properties.outputs.keyVaultName.value -o tsv)

# 2. Securely prompt for the keys (input is hidden)
echo -n "Paste your Groq API Key: "
read -s GROQ_KEY
echo ""

echo -n "Paste your Internal API Key (used for backend authentication): "
read -s INTERNAL_KEY
echo ""

# 3. Set the secrets in Azure Key Vault
az keyvault secret set --vault-name "$KV_NAME" --name "Groq--ApiKey" --value "$GROQ_KEY" -o none
az keyvault secret set --vault-name "$KV_NAME" --name "InternalApiKey" --value "$INTERNAL_KEY" -o none

echo "✅ AI Secrets successfully added to Key Vault!"
```

### 3. 🚀 Deploy Backend (Docker Hub → AKS)
- **What it does:** Builds your .NET API, pushes to Docker Hub, and deploys it to your AKS cluster. It also mounts all Key Vault secrets via the CSI Driver.
- **Wait for it to finish.**

### 4. 🤖 Deploy AI Engine (Docker Hub → AKS)
- **What it does:** Builds your Python FastAPI AI Engine (with sentence-transformers baked in), pushes to Docker Hub, and deploys it to AKS as an internal service. Provisions a **1 Gi PVC** for ChromaDB.
- **Wait for it to finish.**

### 5. 🔄 Automatically Update API URLs
Because the NGINX Ingress Controller provisions a brand new Public IP every time it's installed, you must update your GitHub variables with the new IP before building the frontend.
```bash
./update_github_urls.sh
```
*(This script fetches the new NGINX IP and updates the URLs in GitHub automatically).*

### 6. 🌐 Deploy Frontend (Azure Static Web Apps)
- **What it does:** Builds your Angular frontend using the newly updated URLs and deploys it.
- **Wait for it to finish.**

App is live at your Azure Static Web Apps URL!

---

## Useful Commands

```bash
# Watch pods start up
kubectl get pods -n lms -w

# View Backend API logs
kubectl logs -n lms deployment/lms-api -f

# View AI Engine logs
kubectl logs -n lms deployment/lms-ai-engine -f

# Describe a pod (great for debugging CrashLoopBackOff or FailedMounts)
kubectl describe pod <pod-name> -n lms

# Confirm secrets are successfully injected inside the containers
kubectl exec -n lms deployment/lms-api -- env | grep Jwt__Key
kubectl exec -n lms deployment/lms-ai-engine -- env | grep GROQ_API_KEY

# Force pods to restart (picks up new secrets/configs)
kubectl delete pod -n lms -l app=lms-api
kubectl delete pod -n lms -l app=lms-ai-engine

# Check Ingress IP
kubectl get svc -n ingress-nginx ingress-nginx-controller

# Check ChromaDB PVC status
kubectl get pvc -n lms

# Check NGINX Ingress Logs
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller -f
```

---

## Troubleshooting

### Error: `service account tokens not found`
If your `lms-api` or `lms-ai-engine` pods get stuck in `ContainerCreating` and you see a `FailedMount` error mentioning `service account tokens not found` when you `kubectl describe pod`, this means the CSI Driver failed to configure token requests for Azure Workload Identity during installation.

**Fix:** Run the following command in your local terminal to manually patch the CSI Driver and then delete the stuck pods (they will recreate automatically):
```bash
# Patch the CSI driver
kubectl patch csidriver secrets-store.csi.k8s.io --type merge -p '{"spec":{"tokenRequests":[{"audience":"api://AzureADTokenExchange"}]}}'

# Restart the stuck pods
kubectl delete pod -n lms --all
```

### Error: `The Ingress "lms-api-ingress" is invalid`
If the "Deploy Backend" GitHub Action fails with an error stating the Ingress host `lms-api..nip.io` is invalid, this means the GitHub Action ran *before* Azure finished assigning a Public IP to your NGINX Ingress Controller.

**Fix:** This is usually a timing issue on fresh clusters. Simply wait a few minutes and re-run the "Deploy Backend" GitHub Action. *(Note: The workflow has been updated to automatically wait for the IP to prevent this in the future).*

### Error: API Loading Infinitely (Connection Timeout)
If your API endpoints or `/health` route are loading infinitely in the browser or timing out via curl, Azure Load Balancer may be dropping your traffic due to a misconfigured health probe.

**Fix:** Ensure the NGINX Ingress Controller has the correct health probe annotation. Run this command to fix it on a live cluster:
```bash
kubectl annotate svc ingress-nginx-controller -n ingress-nginx service.beta.kubernetes.io/azure-load-balancer-health-probe-request-path=/healthz --overwrite
```
*(Note: The GitHub Actions infrastructure script has been updated with the proper Bash escaping `\\\` to prevent this from happening on future deployments).*

---

## Architecture

```
YOUR MACHINE (az login — Contributor)
  ./auto_setup.sh
      ├── az group create
      ├── az deployment group create
      ├── az keyvault secret set ×13 (backend)
      ├── az aks get-credentials (clean isolation)
      └── gh secret/variable set (Repo & Environment levels)

GITHUB ACTIONS (no Azure SP at all)
  ☸️ K8s Setup      → kubectl (KUBE_CONFIG_B64) → installs CSI Driver (Workload Identity) + Ingress + Cert-Manager
  🚀 Backend        → docker push (Docker Hub) + kubectl (KUBE_CONFIG_B64)
  🤖 AI Engine      → docker push (Docker Hub) + kubectl (KUBE_CONFIG_B64)
  🌐 Frontend       → SWA deploy (AZURE_STATIC_WEB_APPS_API_TOKEN)

AZURE RESOURCES (coursehublms-rg-dev, centralindia)
  lms-vnet              VNet 10.0.0.0/16
  lms-aks-dev           AKS (1× Standard_B2s, Workload Identity enabled)
  lms-postgres-dev      PostgreSQL 16 (private, VNet-only)
  lms-redis-dev         Redis Basic C0
  lms-kv-dev            Key Vault (access policies, 15 total secrets)
  lmsblobstorage*       Blob Storage (lms-media + lms-public)
  lms-swa-dev           Azure Static Web Apps (Free)
  lms-ai-engine-chroma  Azure Managed Disk PVC 1 Gi (ChromaDB vector store)

DOCKER HUB
  <username>/lms-api:<sha-tag>        ← built by Backend workflow, pulled by AKS
  <username>/lms-ai-engine:<sha-tag>  ← built by AI Engine workflow, pulled by AKS

IN-CLUSTER NETWORKING (Namespace: lms)
  lms-api-svc         ClusterIP :80 → lms-api :8080       (Public via NGINX Ingress)
  lms-ai-engine-svc   ClusterIP :80 → lms-ai-engine :8001  (Internal only)
```