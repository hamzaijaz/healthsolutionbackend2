# functionapp-deploy.ps1

> Used to deploy a build artifact to Azure FunctionApp

## Parameters

In addition to the [Common Script Parameters](common-script-parameters.md), this script also supports the following additional parameters;

| Parameter Name           | Default Value                      | Description                                                    |
| ------------------------ | ---------------------------------- | -------------------------------------------------------------- |
| \$FunctionAppZipFilePath | \$RootPath/functionapp/service.zip | The path to the built, published, and zipped function app code |

## Variables

The following variables are used in this script. The values are defined in the core variables but can be overridden at the application or environment level, see [app-variables-core](app-variables-core.md) for more information.

| Name                      | Default Value                                                                                  | Description                                                                 |
| ------------------------- | ---------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| \${app-DeploymentSlot}    | unstable                                                                                       | slot processing is ignored when _'\${app-DeploymentSlot}'_ = _'production'_ |
| \${app-FunctionAppName}   | ${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName} | the name of the function app                                                |
| \${product-ResourceGroup} | rg-${config-ShortLineOfBusiness}$ShortEnvironment-\${config-ProductName}                       | the resource group for application resources                                |

## Description

Deploys the function app build artifact to the function app or slot.

To create a local deployment artifact;

1. Run _'dotnet publish'_ on your service project
2. Zip the contents of the publish folder _'src/Service/bin/Release/netcoreapp2.1/publish'_ (assuming a 'release' build of a dotnetcore 2.1 project)
3. Copy the zip file into \$FunctionAppZipFilePath
4. Run the script, see [examples](##Examples) below

## Examples

From the root of your application repository assuming the scripts have been added in the _'default'_ location.

### 1. Deploy a new function app build

```powershell
.\deploy\core\scripts\functionapp-deploy.ps1
```
