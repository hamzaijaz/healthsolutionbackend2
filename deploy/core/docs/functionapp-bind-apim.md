# functionapp-bind-apim.ps1

> Used to configure an Azure FunctionApp or Slot

## Parameters

In addition to the [Common Script Parameters](common-script-parameters.md), this script also supports the following additional parameters;

| Parameter Name    | Default Value              | Description                                     |
| ----------------- | -------------------------- | ----------------------------------------------- |
| \$ArmTemplatePath | \$RootPath/deploy/core/arm | The path to the folder containing arm templates |

## Variables

The following variables are used in this script. The values are defined in the core variables but can be overridden at the application or environment level, see [app-variables-core](app-variables-core.md) for more information.

| Name                                   | Default Value                                                                                  | Description                                                                         |
| -------------------------------------- | ---------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| \${config-ProductName}                 | \$null                                                                                         | the product name                                                                    |
| \${config-ShortLineOfBusiness}         | df                                                                                             | a unique 2-3 letter code representing the business unit, e.g.; df - Digital Foundry |
| \${app-DeploymentSlot}                 | unstable                                                                                       | slot processing is ignored when _'\${app-DeploymentSlot}'_ = _'production'_         |
| \${app-Location}                       | australiasoutheast                                                                             | the deployment location for azure resources                                         |
| \${product-ApiManagementName}          | ${config-ShortLineOfBusiness}$ShortEnvironment-api                                             | the name of the apim instance                                                       |
| \${product-ApiManagementResourceGroup} | rg-${config-ShortLineOfBusiness}$ShortEnvironment-api                                          | the resource group containing the apim instance                                     |
| \${product-FunctionAppName}            | ${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName} | the name of the function app                                                        |
| \${product-ResourceGroup}              | rg-${config-ShortLineOfBusiness}$ShortEnvironment-\${config-ProductName}                       | the resource group for application resources                                        |

## Description

The script reads a list of Apis and operations from _\$RootPath/deploy/apim-bindings.json_ to determine what API operations in API Management the function app will be bound to. The operation names _**MUST**_ match exactly with the operationId attribute defined in the open api specification for each API.

APIM binding is done via an ARM template which;

1. Stores the function app host key in APIM
2. Creates an APIM backend that uses the function app host key
3. Applies policies to the api operations to use the backend
4. If an \${app-DeploymentSlot} is configured the policy will support an _'x-ms-routing-name'_ request header containing to direct requests to the required Function App Slot

## Examples

### 1. Sample apim-bindings.json

_\$RootPath/deploy/core/samples/apim-bindings.json_

```json
{
  "apis": [
    {
      "name": "apiName1",
      "operations": [
        {
          "name": "api1Operation1"
        },
        {
          "name": "api1Operation2"
        }
      ]
    },
    {
      "name": "apiName2",
      "operations": [
        {
          "name": "api2Operation1"
        },
        {
          "name": "api2Operation2"
        }
      ]
    }
  ]
}
```
