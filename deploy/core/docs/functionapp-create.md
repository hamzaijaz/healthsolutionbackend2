# functionapp-create.ps1

> Used to create an Azure FunctionApp

## Parameters

This script supports the [Common Script Parameters](common-script-parameters.md)

## Variables

The following variables are used in this script. The values are defined in the core variables but can be overridden at the application or environment level, see [app-variables-core](app-variables-core.md) for more information.

| Name                                       | Default Value                                                                                          | Description                                                                                                                                               |
| ------------------------------------------ | ------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| \${app-ForceCreateFunctionApp}             | \$false                                                                                                | forces the function app to be re-created even if it already exists. **destroys all configuration settings**                                               |
| \${app-FunctionAppName}                    | ${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName}         | the name of the function app                                                                                                                              |
| \${product-ApiManagementVnetResourceGroup} | rg-${config-ShortLineOfBusiness}$ShortEnvironment-network-resources                                    | the resource group containing the apim networking (vnet/subnet). The function app only allows traffic from the apim subnet                                |
| \${product-ApiManagementVnetName}          | ${config-ShortLineOfBusiness}$ShortEnvironment-private-vnet                                            | the virtual network apim is connected to. The function app only allows traffic from the apim subnet                                                       |
| \${product-ApiManagementSubnetName}        | ${config-ShortLineOfBusiness}$ShortEnvironment-private-sn-apim                                         | the subnet of the apim instance. The function app only allows traffic from the apim subnet                                                                |
| \${product-AppInsightsName}                | ${config-ShortLineOfBusiness}$ShortEnvironment-appinsights                                             | the name of the app insights instance                                                                                                                     |
| \${product-AppInsightsResourceGroup}       | rg-${config-ShortLineOfBusiness}$ShortEnvironment-common                                               | the resource group containing the app insights instance                                                                                                   |
| \${product-AppServicePlanName}             | ${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${product-AppServicePlanKind}-asp | the default app service plan used to host function apps. A premium function app plan is required for vnet connectivity                                    |
| \${product-KeyVaultName}                   | ${config-ShortLineOfBusiness}$ShortEnvironment-\${config-ProductName}-kv                               | the function app is given access the read the secrets in this key vault. Holds product secrets, various connection strings, etc                           |
| \${product-NetworkVnet}                    | ${config-ShortLineOfBusiness}$ShortEnvironment-private-vnet                                            | the virtual network the function app is attached to                                                                                                       |
| \${product-NetworkSubnet}                  | ${config-ShortLineOfBusiness}$ShortEnvironment-private-sn-app-\${config-ProductName}                   | the sub net the function app is attached to                                                                                                               |
| \${product-ResourceGroup}                  | rg-${config-ShortLineOfBusiness}$ShortEnvironment-\${config-ProductName}                               | the resource group for application resources                                                                                                              |
| \${product-StorageAccountName}             | ${config-ShortLineOfBusiness}$ShortEnvironment$(${config-ProductName})sa                               | the default storage account used by azure functions to manage triggers, logging, etc. The same storage account is used for all Function Apps in a Product |

## Description

Checks to see if a function app already exists with the same name and creates it if;

1. it doesn't exist

   OR

2. the \${app-ForceCreateFunctionApp} variable = _\$true_

If the function app is created the script performs the following actions;

- assigns a managed identity
- configures key vault access for the managed identity
- attaches the function app to the product subnet
- restricts function app access to only accept requests from Azure API Management
- sets the function app to only accept https requests

## Examples

From the root of your application repository assuming the scripts have been added in the _'default'_ location.

### 1. Create a new function app

```powershell
.\deploy\core\scripts\functionapp-create.ps1
```
