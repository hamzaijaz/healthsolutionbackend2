#Requires -Version 5.0

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [string] $FunctionAppZipFilePath = "$RootPath/functionapp/service.zip",
    [hashtable] $VariableOverrides = @{}
)

$ErrorActionPreference = "stop"

# Logging in Azure Portal
try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("DigitalFoundryDevOps-$UI$($host.name)".replace(" ", "_"), "1.0")
} 
catch { }

Write-Host "STEP: Deploying FunctionApp" -ForegroundColor Green

# # Make sure Az powershell modules are loaded
Import-Module $PSScriptRoot/utils.psm1 -Force

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

if (${app-DeploymentSlot} -ieq "production") {
    Invoke-AzCliCommand -Command {
        az functionapp deployment source config-zip `
            --resource-group ${product-ResourceGroup} `
            --name ${app-FunctionAppName} `
            --src $FunctionAppZipFilePath
    }
}
else {
    Invoke-AzCliCommand -Command {
        az functionapp deployment source config-zip `
            --resource-group ${product-ResourceGroup} `
            --name ${app-FunctionAppName} `
            --src $FunctionAppZipFilePath `
            --slot ${app-DeploymentSlot}
    }

    Invoke-AzCliCommand -Command {
        az functionapp start `
            --resource-group ${product-ResourceGroup} `
            --name ${app-FunctionAppName} `
            --slot ${app-DeploymentSlot}
    }
}
