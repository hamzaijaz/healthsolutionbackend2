#Requires -Version 5.0

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [string] $CoreScriptFolder = "$RootPath/deploy/core/scripts",
    [hashtable] $VariableOverrides = @{}
)

$ErrorActionPreference = "stop" 

# Load Variables
. $CoreScriptFolder/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

# Validate application name to ensure it conforms to allowed Regex pattern - '^[0-9a-zA-Z-]+$'
if ("${config-ApplicationName}" -notmatch '^[0-9a-zA-Z-]+$') {
    throw "The source branch name must conform to the following pattern: '^[0-9a-zA-Z-]+$'"
}

$subscriptionName = az account show --query name --output tsv

Write-Host "INFO: Create Integration test database" -ForegroundColor Green

. $RootPath/deploy/core/scripts/sql-deploy-scripts.ps1 `
                                -ShortEnvironment $ShortEnvironment `
                                -RootPath $RootPath `
                                -CustomScriptPath $CustomScriptPath `
                                -CoreScriptFolder $CoreScriptFolder `
                                -VariableOverrides $VariableOverrides

Write-Host "INFO: Get connectionstring from vault" -ForegroundColor Green

# Retrieve Key Vault Secret
$ConnectionString = az keyvault secret show `
                            --subscription $subscriptionName `
                            --vault-name ${product-KeyVaultName} `
                            --name "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName}-sql-connectionstring" `
                            --query value `
                            -o tsv

Write-Host "INFO: Set test project integration test db settings" -ForegroundColor Green

$configFile = "$RootPath/tests/Application.IntegrationTests/appsettings.json"
if (Test-Path -Path $configFile) {
    $config = (Get-Content -Raw -Path "$configFile" | ConvertFrom-Json)
    $config.ConnectionString = $ConnectionString
    $json = $config | ConvertTo-Json
    #Write-Host $json
    Set-Content -Path $configFile -Value $json -Force
}
else {
    throw "Could not find integration tests config file - $configFile"
}