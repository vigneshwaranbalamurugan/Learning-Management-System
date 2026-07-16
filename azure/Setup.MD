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
- **What it does:** Installs CSI Driver, NGINX Ingress Controller, and Cert-Manager.
- **Wait for it to finish.**

### 2. 🚀 Deploy Backend (Docker Hub → AKS)
- **What it does:** Builds your .NET API, pushes to Docker Hub, and deploys it to your AKS cluster.
- **Wait for it to finish.**

### 3. Automatically Update API URLs
Because the NGINX Ingress Controller provisions a brand new Public IP every time it's installed, you must update your GitHub variables with the new IP before building the frontend.
```bash
./update_github_urls.sh
```
*(This script fetches the new NGINX IP and updates the URLs in GitHub automatically).*

### 4. 🌐 Deploy Frontend (Azure Static Web Apps)
- **What it does:** Builds your Angular frontend using the newly updated URLs and deploys it.
- **Wait for it to finish.**

App is live at your Azure Static Web Apps URL!

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

# Check Ingress IP
kubectl get svc -n ingress-nginx ingress-nginx-controller

# Check NGINX Ingress Logs
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller -f

```

---

## Architecture

```
YOUR MACHINE (az login — Contributor)
  ./auto_setup.sh
      ├── az group create
      ├── az deployment group create
      ├── az keyvault secret set ×13
      ├── az aks get-credentials
      └── gh secret/variable set

GITHUB ACTIONS (no Azure SP at all)
  ☸️ K8s Setup    → kubectl (KUBE_CONFIG_B64) → installs CSI Driver + Ingress + Cert-Manager
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
  <username>/lms-api:<sha-tag>        ← built by Backend workflow, pulled by AKS
  <username>/lms-ai-engine:<sha-tag>  ← built by AI Engine workflow, pulled by AKS
```

---

## Step 3 — Deploy the AI Engine

The AI engine (`ai-engine/`) is a FastAPI service that provides the RAG AI Tutor, lesson
summarisation, and audio transcription features. It runs as an **internal-only** pod inside
AKS — it is never exposed to the internet.

### 3.1 — Add AI Engine secrets to Key Vault (one-time, manual)

Run these two commands from your local machine (after `az login`):

```bash
# Replace <KEY_VAULT_NAME> with the value printed by auto_setup.sh
# or: az keyvault list -g lms-rg-dev --query "[].name" -o tsv

# 1. Groq API key — get yours at https://console.groq.com/keys
az keyvault secret set \
  --vault-name <KEY_VAULT_NAME> \
  --name "Groq--ApiKey" \
  --value "<your-groq-api-key>"

# 2. Internal API key — must match INTERNAL_API_KEY used by the .NET backend
#    Use the same value you put in your root .env file.
az keyvault secret set \
  --vault-name <KEY_VAULT_NAME> \
  --name "InternalApiKey" \
  --value "<your-internal-api-key>"
```

> **Why two separate secrets?**
> `Groq--ApiKey` is AI-engine-specific. `InternalApiKey` is the shared token the .NET
> backend sends in the `X-Internal-Api-Key` header to authenticate all AI engine requests.

### 3.2 — Run the AI Engine deployment workflow

Go to your repository **Actions** tab and run:

**🤖 Deploy AI Engine (Docker Hub → AKS)**

- Builds `ai-engine/Dockerfile` (sentence-transformers model baked in at build time)
- Pushes to Docker Hub as `<username>/lms-ai-engine:<sha-tag>`
- Applies `azure/k8s/ai-engine-*.yaml` manifests to the cluster
- Provisions a **1 Gi Azure Managed Disk PVC** for ChromaDB persistence
- Verifies the `/health` endpoint via port-forward before marking success

### 3.3 — Verify

```bash
# Check the AI engine pod is running
kubectl get pods -n lms -l app=lms-ai-engine

# Check ChromaDB PVC is bound (Azure Managed Disk)
kubectl get pvc lms-ai-engine-chroma-pvc -n lms

# Tail AI engine logs
kubectl logs -n lms deployment/lms-ai-engine -f

# Health check via port-forward (from your local machine)
kubectl port-forward svc/lms-ai-engine-svc 8001:80 -n lms &
curl http://localhost:8001/health
# Expected: {"status": "ok", "service": "lms-ai-engine"}

# Test internal connectivity from the .NET API pod
kubectl exec -it deploy/lms-api -n lms -- \
  curl -H "X-Internal-Api-Key: <your-internal-api-key>" \
  http://lms-ai-engine-svc/health
```

---

## Useful Commands

```bash
# Watch pods start up
kubectl get pods -n lms -w

# View API logs
kubectl logs -n lms deployment/lms-api -f

# View AI Engine logs
kubectl logs -n lms deployment/lms-ai-engine -f

# Confirm secrets are injected
kubectl exec -n lms deployment/lms-api -- env | grep Jwt__Key
kubectl exec -n lms deployment/lms-ai-engine -- env | grep GROQ_API_KEY

# Force pod restart
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

## Architecture

```
YOUR MACHINE (az login — Contributor)
  ./auto_setup.sh
      ├── az group create
      ├── az deployment group create
      ├── az keyvault secret set ×13 (backend) + ×2 (AI engine: Groq--ApiKey, InternalApiKey)
      ├── az aks get-credentials
      └── gh secret/variable set

GITHUB ACTIONS (no Azure SP at all)
  ☸️ K8s Setup      → kubectl (KUBE_CONFIG_B64) → installs CSI Driver + Ingress + Cert-Manager
  🚀 Backend        → docker push (Docker Hub) + kubectl (KUBE_CONFIG_B64)
  🤖 AI Engine      → docker push (Docker Hub) + kubectl (KUBE_CONFIG_B64)
  🌐 Frontend       → SWA deploy (AZURE_STATIC_WEB_APPS_API_TOKEN)

AZURE RESOURCES (coursehublms-rg-dev, centralindia)
  lms-vnet              VNet 10.0.0.0/16
  lms-aks-dev           AKS (1× Standard_B2s, Workload Identity enabled)
  lms-postgres-dev      PostgreSQL 16 (private, VNet-only)
  lms-redis-dev         Redis Basic C0
  lms-kv-dev            Key Vault (access policies, 15 secrets)
  lmsblobstorage*       Blob Storage (lms-media + lms-public)
  lms-swa-dev           Azure Static Web Apps (Free)
  lms-ai-engine-chroma  Azure Managed Disk PVC 1 Gi (ChromaDB vector store)

DOCKER HUB
  <username>/lms-api:<sha-tag>        ← built by Backend workflow, pulled by AKS
  <username>/lms-ai-engine:<sha-tag>  ← built by AI Engine workflow, pulled by AKS

IN-CLUSTER NETWORKING
  lms-api-svc         ClusterIP :80 → lms-api :8080       (public via Ingress)
  lms-ai-engine-svc   ClusterIP :80 → lms-ai-engine :8001  (internal only)
```