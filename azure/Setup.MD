az group create \
  --name coursehub-rg \
  --location centralindia

az deployment group create \
  --resource-group coursehub-rg \
  --template-file blob.bicep \
  --parameters storageAccountName=vigicoursehub30


  az storage account keys list \
  --resource-group coursehub-rg \
  --account-name vigicoursehub30



  