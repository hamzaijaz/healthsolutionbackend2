# app-variables-core.ps1

> defines the names of Azure resources used in the scripts

## Description

In addition to defining the default resource names used in the scripts, this script also applies any application or environment overrides to the default values.

The script;

1. loads the [Script Configuration](script-configuration.md) from the \$CustomScriptPath _('./deploy/script-config.ps1' by default)_
2. uses the script configuration values to set default resource names
3. loads application level variable values from the \$CustomScriptPath _('./deploy/app-variables-default.ps1' by default)_
4. loads environment level variable values from the \$CustomScriptPath _('./deploy/app-variables-{\$ShortEnvironment}.ps1' by default)_
5. loads variable overrides supplied on the command line

### Variable Overrides

Core variables can be overridden at the application or environment level.  To override a core variable, create a _'default'_ or _'environment'_ variable overrides file in the \$CustomScriptPath _(./deploy by default)_

The override files use the following naming convention;

| Override Level | File Name Convention              |
| -------------- | --------------------------------- |
| Application    | app-variables-default.ps1         |
| Environment    | app-variables-_{environment}_.ps1 |
