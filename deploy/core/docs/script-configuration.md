# script-config.ps1

> Sets values used to apply naming conventions for Azure Resources

## Description

The following variables must be defined in a _'script-config.ps1'_ located in the CustomScriptPath _('./deploy' by default)_.

- config-ApplicationName
- config-ProductName
- config-ProductCode
- config-ShortLineOfBusiness

Validation is included in all scripts as part of loading the core variables _('app-variables-core.ps1')_ to ensure values have been configured.

## Examples

A sample _'script-config.ps1'_ can be found in the samples/scripts folder.

```powershell
${config-ApplicationName} = "myservice" # payment, entitlement, communication, etc
${config-ProductName} = "myapp" # capitalraising, crib, ibis, wealth, etc
${config-ProductCode} = ${config-ProductName}
${config-ShortLineOfBusiness} = "df" # Digital Foundry
```
