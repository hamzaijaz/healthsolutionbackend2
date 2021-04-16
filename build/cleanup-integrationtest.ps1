#Requires -Version 5.0

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [string] $CoreScriptFolder = "$RootPath/deploy/core/scripts",
    [hashtable] $VariableOverrides = @{}
)

# Load Variables
. $CoreScriptFolder/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

$subscriptionName = az account show --query name --output tsv

Write-Host "INFO: Cleanup database" -ForegroundColor Green

az sql db delete --name ${app-DatabaseName} --resource-group ${product-ResourceGroup} --server ${product-SqlServerShortName} --yes

Write-Host "INFO: Cleanup database connectionstring" -ForegroundColor Green

# Note: Suppress warning about soft-delete
az keyvault secret delete `
        --subscription $subscriptionName `
        --vault-name ${product-KeyVaultName} `
        --name "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName}-sql-connectionstring" `
        --only-show-errors

# Give Azure time to delete the secret
# Start-Sleep -s 30

# az keyvault secret purge `
#         --subscription $subscriptionName `
#         --vault-name ${product-KeyVaultName} `
#         --name "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName}-sql-connectionstring" `
#         --only-show-errors
