# Replace or override any of the core product variables found in the foundry-core/infrastrcuture repo here
#  https://dev.azure.com/cpu-digital-foundry/foundry-core/_git/infrastructure?path=%2Fproduct%2Fproduct-variables-core.ps1

${product-NetworkVnet} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-private-vnet"
${product-NetworkSubnet} = "default"
${product-NetworkResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}"
${product-ServiceBusName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-servicebus"
${product-ServiceBusResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}"
${product-KeyVaultName} = "${config-ShortLineOfBusiness}$ShortEnvironment${config-ProductName}kv"

${product-ApiManagementName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-api"
${product-ApiManagementResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}"
${product-ApiManagementVnetResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}"
${product-ApiManagementVnetName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-private-vnet"
${product-ApiManagementSubnetName} = "default"
${product-AppInsightsResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}"
${product-AppInsightsName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-appinsights"