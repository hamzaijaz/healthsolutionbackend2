#Requires -Version 5.0

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [hashtable] $VariableOverrides = @{},
    [string] $TargetSlot = 'production'
)

$ErrorActionPreference = "stop"

# Logging in Azure Portal
try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("DigitalFoundryDevOps-$UI$($host.name)".replace(" ", "_"), "1.0")
} 
catch { }

Write-Host "STEP: Swapping FunctionApp Slots" -ForegroundColor Green

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

if (${app-DeploymentSlot} -ine "production") {
    
    Write-Host "INFO: Starting Slot (${app-DeploymentSlot})" -ForegroundColor Yellow
    az functionapp start `
        --resource-group ${product-ResourceGroup} `
        --name ${app-FunctionAppName} `
        --slot ${app-DeploymentSlot}

    Write-Host "INFO: Starting Swap (${app-DeploymentSlot}) <-> (production)" -ForegroundColor Yellow
    az functionapp deployment slot swap `
        --resource-group ${product-ResourceGroup} `
        --name ${app-FunctionAppName} `
        --slot ${app-DeploymentSlot} `
        --target-slot $TargetSlot

    Write-Host "INFO: Stopping Slot (${app-DeploymentSlot})" -ForegroundColor Yellow
    az functionapp stop `
        --resource-group ${product-ResourceGroup} `
        --name ${app-FunctionAppName} `
        --slot ${app-DeploymentSlot} `
        
}
else {
    Write-Host "WARNING: STEP IGNORED: Slot not configured" -ForegroundColor Yellow
}