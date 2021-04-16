# functionapp-create-slot.ps1

> Used to create an Azure FunctionApp Slot

## Parameters

This script supports the [Common Script Parameters](common-script-parameters.md)

## Variables

The following variables are used in this script. The values are defined in the core variables but can be overridden at the application or environment level, see [app-variables-core](app-variables-core.md) for more information.

| Name                                       | Default Value                                                                                  | Description                                                                                                                     |
| ------------------------------------------ | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| \${app-DeploymentSlot}                     | unstable                                                                                       | slot processing is ignored when _'\${app-DeploymentSlot}'_ = _'production'_                                                     |
| \${app-ForceCreateFunctionApp}             | \$false                                                                                        | forces the function app to be re-created even if it already exists. **destroys all configuration settings**                     |
| \${app-FunctionAppName}                    | ${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName} | the name of the function app                                                                                                    |
| \${product-ApiManagementVnetResourceGroup} | rg-${config-ShortLineOfBusiness}$ShortEnvironment-network-resources                            | the resource group containing the apim instance. The function app only allows traffic from the apim subnet                      |
| \${product-ApiManagementVnetName}          | ${config-ShortLineOfBusiness}$ShortEnvironment-private-vnet                                    | the virtual network apim is connected to. The function app only allows traffic from the apim subnet                             |
| \${product-ApiManagementSubnetName}        | ${config-ShortLineOfBusiness}$ShortEnvironment-private-sn-apim                                 | the subnet of the apim instance. The function app only allows traffic from the apim subnet                                      |
| \${product-KeyVaultName}                   | ${config-ShortLineOfBusiness}$ShortEnvironment-\${config-ProductName}-kv                       | the function app is given access the read the secrets in this key vault. Holds product secrets, various connection strings, etc |
| \${product-ResourceGroup}                  | rg-${config-ShortLineOfBusiness}$ShortEnvironment-\${config-ProductName}                       | the resource group for application resources                                                                                    |

## Description

When _'\${app-DeploymentSlot}'_ != _'production'_, the script checks to see if a slot already exists in the function app with the same name and creates it if;

1. it doesn't exist

   OR

2. the \${app-ForceCreateFunctionApp} variable = _\$true_

If the function app slot is created the script performs the following actions;

- assigns a managed identity
- configures key vault access for the managed identity
- attaches the function app to the product subnet
- restricts function app access to only accept requests from Azure API Management

## Examples

### 1. Create a Function App Slot

```powershell
${app-DeploymentSlot} = "unstable"

.\deploy\core\scripts\functionapp-create-slot.ps1
```

### 2. Bypass Function App Slot creation

```powershell
${app-DeploymentSlot} = "production"

.\deploy\core\scripts\functionapp-create-slot.ps1
```
