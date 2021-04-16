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

Write-Host "STEP: Creating FunctionApp" -ForegroundColor Green

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

# Check to see if the function app exists                   
$functionAppExists = az functionapp show `
                        --resource-group ${product-ResourceGroup} `
                        --name ${app-FunctionAppName} `
                        --query name

# If the function app already exists, don't re-create it
# This is NOT idempotent and will remove any non default app settings (connection strings, etc)
if (!$functionAppExists -or ${app-ForceCreateFunctionApp})
{
    Write-Host "INFO: Get AppInsights Instrumentation Key" -ForegroundColor Yellow
    $appInsightsKey = az resource show `
                            --resource-group ${product-AppInsightsResourceGroup} `
                            --name ${product-AppInsightsName} `
                            --resource-type "Microsoft.Insights/components" `
                            --query properties.InstrumentationKey

    Write-Host "INFO: ${app-FunctionAppName}: Create FunctionApp" -ForegroundColor Yellow
    az functionapp create `
        --name ${app-FunctionAppName} `
        --resource-group ${product-ResourceGroup} `
        --storage-account ${product-StorageAccountName} `
        --plan ${product-AppServicePlanName} `
        --app-insights-key $appInsightsKey `
        --runtime dotnet `
        --functions-version ${app-FunctionAppRuntimeVersion} `
        --output none

    Write-Host "INFO: ${app-FunctionAppName}: Assign Managed Identity" -ForegroundColor Yellow
    $functionAppPrincipalId = az functionapp identity assign `
                                --name ${app-FunctionAppName} `
                                --resource-group ${product-ResourceGroup} `
                                --query principalId `
                                --output tsv

    Write-Host "INFO: ${app-FunctionAppName}: Configure KeyVault Access" -ForegroundColor Yellow
    az keyvault set-policy `
        --name ${product-KeyVaultName} `
        --secret-permissions get list `
        --object-id $functionAppPrincipalId `
        --resource-group ${product-ResourceGroup} `
        --output none

    # TODO: should this be 'only run on create'?
    #       depends on when/if the vnet will change
    #       if it does the impact is likely to be much larger than just this
    Write-Host "INFO: ${app-FunctionAppName}: Configure Vnet Integration" -ForegroundColor Yellow
    az functionapp vnet-integration add `
        --name ${app-FunctionAppName} `
        --resource-group ${product-ResourceGroup} `
        --vnet ${product-NetworkVnet} `
        --subnet ${product-NetworkSubnet} `
        --output none

    # TODO: should this be the default?
    #       makes it difficult to test / verify function apps
    #       forces an API first approach (not a bad thing?)
    Write-Host "INFO: ${app-FunctionAppName}: Configure access to allow APIM only" -ForegroundColor Yellow
    $apimSubnetId = az network vnet subnet show `
                        --resource-group ${product-ApiManagementVnetResourceGroup} `
                        --vnet-name ${product-ApiManagementVnetName} `
                        --name ${product-ApiManagementSubnetName} `
                        --query id
    
    az webapp config access-restriction add `
        --name ${app-FunctionAppName} `
        --resource-group ${product-ResourceGroup} `
        --action Allow `
        --rule-name AllowAPIMOnly `
        --subnet $apimSubnetId `
        --priority 300 `
        --output none

    Write-Host "INFO: ${app-FunctionAppName}: Configure Https only" -ForegroundColor Yellow
    az functionapp update `
        --name ${app-FunctionAppName} `
        --resource-group ${product-ResourceGroup} `
        --set HttpsOnly=true `
        --output none
}
else {
    Write-Host "INFO: FunctionApp Exists and ForceCreateFunctionApp = false" -ForegroundColor Yellow
}
