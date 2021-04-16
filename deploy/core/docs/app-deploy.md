# app-deploy.ps1

> the entry point when running the script in an Azure DevOps pipeline

## Parameters

In addition to the [Common Script Parameters](common-script-parameters.md), this script also supports the following additional parameters;

| Parameter Name           | Default Value                      | Description                                                     |
| ------------------------ | ---------------------------------- | --------------------------------------------------------------- |
| \$SqlScriptPath          | \$RootPath/sql                     | The path to the folder containing sql scripts to execute        |
| \$FunctionAppZipFilePath | \$RootPath/functionapp/service.zip | The path to the built, published, and zipped function app code  |
| \$ArmTemplatePath        | \$RootPath/deploy/core/arm         | The path to the folder containing arm templates                 |

## Description

The is a wrapper to execute the various scripts in order to produce a full deployment pipeline. The script execution order is;

1. [functionapp-create.ps1](functionapp-create.md)
2. [functionapp-create-slot.ps1](functionapp-create-slot.md)
3. [sql-deploy-scripts.ps1](sql-deploy-scripts.md)
4. [functionapp-configure-settings.ps1](functionapp-configure-settings.md)
5. [servicebus-configure.ps1](servicebus-configure.md)
6. [functionapp-deploy.ps1](functionapp-deploy.md)
7. [functionapp-bind-apim.ps1](functionapp-bind-apim.md)
8. [app-custom-script.ps1](app-custom-script.md)
