#!/bin/bash

# Ensure the script stops on errors
set -e

echo "🔍 Fetching NGINX Ingress Controller IP from AKS..."
NGINX_IP=$(kubectl get svc -n ingress-nginx ingress-nginx-controller -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

if [ -z "$NGINX_IP" ] || [ "$NGINX_IP" == "<pending>" ]; then
    echo "❌ NGINX IP is not yet available. Please wait a few minutes and try again."
    exit 1
fi

echo "✅ Found IP: $NGINX_IP"

# Generate the URLs
APIURL="https://lms-api.${NGINX_IP}.nip.io/api/v1"
HUBURL="https://lms-api.${NGINX_IP}.nip.io/hubs"
HANGFIREURL="https://lms-api.${NGINX_IP}.nip.io/hangfire"

echo "🔄 Updating GitHub Variables..."

gh variable set AKS_EXTERNAL_IP -b "$NGINX_IP"
gh variable set APIURL -b "$APIURL"
gh variable set HUBURL -b "$HUBURL"
gh variable set HANGFIREURL -b "$HANGFIREURL"

echo "🎉 GitHub Variables updated successfully!"
echo ""
echo "Next Steps:"
echo "1. Run the 'Deploy Backend' workflow"
echo "2. Run the 'Deploy Frontend' workflow"
