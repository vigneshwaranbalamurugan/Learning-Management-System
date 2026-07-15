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
  <username>/lms-api:<sha-tag>  ← built by GitHub Actions, pulled by AKS
```