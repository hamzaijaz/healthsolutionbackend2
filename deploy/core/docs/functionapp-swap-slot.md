# functionapp-create-slot.ps1

> Used to create an Azure FunctionApp Slot

## Parameters

In addition to the [Common Script Parameters](common-script-parameters.md), this script also supports the following additional parameters;

| Parameter Name | Default Value | Description           |
| -------------- | ------------- | --------------------- |
| \$TargetSlot   | production    | The slot to swap into |

## Variables

The following variables are used in this script. The values are defined in the core variables but can be overridden at the application or environment level, see [app-variables-core](app-variables-core.md) for more information.

| Name                      | Default Value                                                                                  | Description                                                                                  |
| ------------------------- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| \${app-DeploymentSlot}    | unstable                                                                                       | source slot for the swap operation, ignored when _'\${app-DeploymentSlot}'_ = _'production'_ |
| \${app-FunctionAppName}   | ${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName} | the name of the function app                                                                 |
| \${product-ResourceGroup} | rg-${config-ShortLineOfBusiness}$ShortEnvironment-\${config-ProductName}                       | the resource group for application resources                                                 |

## Description

When _'\${app-DeploymentSlot}'_ != _'production'_, the script will swap the code deployed in the _'\${app-DeploymentSlot}'_ and \$TargetSlot.

## Examples

### 1. Swap the 'unstable' slot with 'production'

```powershell
${app-DeploymentSlot} = "unstable"

.\deploy\core\scripts\functionapp-swap-slot.ps1
```

### 2. Bypass slot swap

```powershell
${app-DeploymentSlot} = "production"

.\deploy\core\scripts\functionapp-swap-slot.ps1
```
