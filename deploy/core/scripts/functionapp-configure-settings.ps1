#Requires -Version 5.0
#Requires -Module Az.Accounts
#Requires -Module Az.Resources
#Requires -Module Az.KeyVault

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [hashtable] $VariableOverrides = @{}
)

function ApplyFunctionAppSettings {
    param (
        [string] $SettingsFilePath
    )
    
    # Apply any additional settings to each slot
    if (Test-Path -Path "$SettingsFilePath") {
        
        Write-Host "Applying functionapp settings from: $SettingsFilePath"

        $settingsJson = Get-Content -Raw -Path "$SettingsFilePath" | ConvertFrom-Json
        
        $functionAppSettings = @()

        foreach ($setting in $settingsJson.settings) {
            # Expands any variables defined in the functionapp-settings.json
            $settingName = $ExecutionContext.InvokeCommand.ExpandString($($setting.name))
            $settingValue = $ExecutionContext.InvokeCommand.ExpandString($($setting.value))

            # If the setting is a key vault reference, get the reference, else just use the value
            if ($setting.isKeyVaultReference) {
                $keyVaultRef = az keyvault secret show `
                                    --name $settingValue `
                                    --vault-name ${product-KeyVaultName} `
                                    --query "id"

                $settingValue = "@Microsoft.KeyVault(SecretUri=$keyVaultRef^^)"
            }

            $functionAppSettings += @("$settingName=$settingValue")
        }

        if (${app-DeploymentSlot} -ieq "production") {
            az functionapp config appsettings set `
                --resource-group ${product-ResourceGroup} `
                --name ${app-FunctionAppName} `
                --settings $functionAppSettings `
                --output none
        }
        else {
            az functionapp config appsettings set `
                --resource-group ${product-ResourceGroup} `
                --name ${app-FunctionAppName} `
                --slot ${app-DeploymentSlot} `
                --settings $functionAppSettings `
                --output none
        }
    }
    else {
        Write-warning "No additional functionapp settings found: $SettingsFilePath"
    }
}


# SCRIPT BEGIN
$ErrorActionPreference = "stop"

# Logging in Azure Portal
try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("DigitalFoundryDevOps-$UI$($host.name)".replace(" ", "_"), "1.0")
} 
catch { }

Write-Host "STEP: Configuring FunctionApp Settings" -ForegroundColor Green

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides


# Apply core functionapp settings
ApplyFunctionAppSettings -SettingsFilePath "$PSScriptRoot/functionapp-settings-core.json"

# Apply application specific functionapp settings
ApplyFunctionAppSettings -SettingsFilePath "$CustomScriptPath/functionapp-settings.json"
