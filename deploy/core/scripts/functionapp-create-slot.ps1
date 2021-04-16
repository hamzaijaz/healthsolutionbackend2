#Requires -Version 5.0

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [hashtable] $VariableOverrides = @{}
)

$ErrorActionPreference = "stop"

# Logging in Azure Portal
try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("DigitalFoundryDevOps-$UI$($host.name)".replace(" ", "_"), "1.0")
} 
catch { }

Write-Host "STEP: Creating FunctionApp Slot" -ForegroundColor Green

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

if (${app-DeploymentSlot} -ine "production") {

    $slots = az functionapp deployment slot list `
                --name ${app-FunctionAppName} `
                --resource-group ${product-ResourceGroup} `
                | ConvertFrom-Json

    $slot = $slots | Where-Object { $_.name -eq "${app-DeploymentSlot}" }

    if (($null -eq $slot) -or ${app-ForceCreateFunctionApp} ) {
        # Slot creation is idempotent, functionapp creation isn't
        # Re-creating a slot retains existing app settings (connection strings, etc)
        Write-Host "INFO: ${app-FunctionAppName}-$(${app-DeploymentSlot}): Create Slot" -ForegroundColor Yellow
        az functionapp deployment slot create `
            --name ${app-FunctionAppName} `
            --resource-group ${product-ResourceGroup} `
            --slot ${app-DeploymentSlot} `
            --output none

        Write-Host "INFO: ${app-FunctionAppName}-$(${app-DeploymentSlot}): Assign Managed Identity" -ForegroundColor Yellow
        $functionAppPrincipalId = az functionapp identity assign `
                                    --name ${app-FunctionAppName} `
                                    --slot ${app-DeploymentSlot} `
                                    --resource-group ${product-ResourceGroup} `
                                    --query principalId `
                                    --output tsv

        Write-Host "INFO: ${app-FunctionAppName}-$(${app-DeploymentSlot}): Configure KeyVault Access" -ForegroundColor Yellow
        az keyvault set-policy `
            --name ${product-KeyVaultName} `
            --secret-permissions get list `
            --object-id $functionAppPrincipalId `
            --resource-group ${product-ResourceGroup} `
            --output none

        Write-Host "INFO: ${app-FunctionAppName}-$(${app-DeploymentSlot}): Configure access to allow APIM only" -ForegroundColor Yellow
        $apimSubnetId = az network vnet subnet show `
                            --resource-group ${product-ApiManagementVnetResourceGroup} `
                            --vnet-name ${product-ApiManagementVnetName} `
                            --name ${product-ApiManagementSubnetName} `
                            --query id
        
        az webapp config access-restriction add `
            --name ${app-FunctionAppName} `
            --slot ${app-DeploymentSlot} `
            --resource-group ${product-ResourceGroup} `
            --action Allow `
            --rule-name AllowAPIMOnly `
            --subnet $apimSubnetId `
            --priority 300 `
            --output none
    }
    else {
        Write-Host "INFO: FunctionApp Slot Exists and ForceCreateFunctionApp = false" -ForegroundColor Yellow
    }
}