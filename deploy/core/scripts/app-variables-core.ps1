#Requires -Version 5.0

Param(
    [string] $CustomScriptPath = "./deploy",
    [string] $ShortEnvironment = "local",
    [hashtable] $VariableOverrides = @{}
)

function Confirm-ScriptConfiguration {
    Param(
        [Parameter(Mandatory=$true)]
        [string[]] $VariableNames
    )

    foreach ($variableName in $VariableNames) {
        $value = Get-Variable -Name $variableName -ValueOnly -ErrorAction SilentlyContinue

        if ($null -eq $value) {
            if ($null -eq $missingVariables) {
                $missingVariables = "$variableName"
            }
            else {
                $missingVariables = $missingVariables, "$variableName"
            }
        }
    }

    if ($null -ne $missingVariables) {
        throw "Variables ($missingVariables) are not set in; '$CustomScriptPath/script-config.ps1'.  Sample configuration can be found in; '$RootPath/deploy/core/samples/scripts'"
    }
    
}
### 
# NOTE: All variables defined in this script can be replaced 
#       with application/environment specific values if required
#
#       Refer to the end of this script for filename location
###

### 
# Configuration Variables
# - These $null values below MUST be set in a variable overrides file;
#     $CustomScriptPath/app-variables-default.ps1
###
$scriptConfig = "$CustomScriptPath/script-config.ps1"

if (Test-Path -Path $scriptConfig) {
    Write-Host "INFO: Loading script configuration; $scriptConfig" -ForegroundColor Yellow

    . $scriptConfig
}
else {
    throw "Script configuration file not found; $scriptConfig"
}

Confirm-ScriptConfiguration "config-ApplicationName", "config-ProductName", "config-ProductCode", "config-ShortLineOfBusiness"

###
# Product Variables
# - Duplicated here for ease of use, not sure about long term maintenance
###
# General
${product-ResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}"

# AppInsights
${product-AppInsightsResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-common"
${product-AppInsightsName} = "${config-ShortLineOfBusiness}$ShortEnvironment-appinsights"

# AppServicePlan
${product-AppServicePlanKind} = "elastic"
${product-AppServicePlanName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${product-AppServicePlanKind}-asp"

# Api Management
${product-ApiManagementName} = "${config-ShortLineOfBusiness}$ShortEnvironment-api"
${product-ApiManagementResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-api"
${product-ApiManagementVnetResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-network-resources"
${product-ApiManagementVnetName} = "${config-ShortLineOfBusiness}$ShortEnvironment-private-vnet"
${product-ApiManagementSubnetName} = "${config-ShortLineOfBusiness}$ShortEnvironment-private-sn-apim"

# KeyVault
${product-KeyVaultName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-kv"

# Network
${product-NetworkVnet} = "${config-ShortLineOfBusiness}$ShortEnvironment-private-vnet"
${product-NetworkSubnet} = "${config-ShortLineOfBusiness}$ShortEnvironment-private-sn-app-${config-ProductName}"

# ServiceBus
${product-ServiceBusName} = "${config-ShortLineOfBusiness}$ShortEnvironment-servicebus"
${product-ServiceBusResourceGroup} = "rg-${config-ShortLineOfBusiness}$ShortEnvironment-common"
${product-ServiceBusDefaultMaxSize} = 1024
${product-ServiceBusDefaultMaxDeliveryCount} = 10

# Sql
${product-SqlServerEnvironment} = "$ShortEnvironment"
${product-SqlServerAdminUsername} = "sql-admin-${config-ProductName}"
${product-SqlServerFullName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-sql.database.windows.net,1433"
${product-SqlServerShortName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-sql"
${product-SqlServerElasticPool} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-sql-pool"

# StorageAccount
${product-StorageAccountName} = "${config-ShortLineOfBusiness}$ShortEnvironment$(${config-ProductName})sa"

###
# Application Variables
###

# Switches (everything is opt-in by default)
#   - effectively forces every product to define their own variables to override

# Configuration
${app-Location} = "australiasoutheast"

# FunctionApp
${app-FunctionAppName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName}"
${app-DeploymentSlot} = "unstable"
${app-ForceCreateFunctionApp} = $false
# Backwards compatible
${app-FunctionAppRuntimeVersion} = 2 

# SQL 
${app-DatabaseName} = "$(${config-ProductCode})_$(${config-ApplicationName})_$ShortEnvironment"
${app-SqlSchemaVersionsTable} = "SchemaVersions_$(${config-ProductName})_$(${config-ApplicationName})"
${app-DbRoleServiceUsersMember} = "${config-ApplicationName}svc_$ShortEnvironment"
${secret-DbRoleServiceUsersPassword} = "dummy"
${app-DbRoleTestServiceUsersMember} = "NotSet"
${app-DbRoleSupportUsersMember} = "NotSet"

# Load Product Specific Overrides
$customVariables = "$CustomScriptPath/app-variables-default.ps1"

if (Test-Path -Path $customVariables) {
    Write-Host "INFO: Loading custom app variables; $customVariables" -ForegroundColor Yellow

    . $customVariables
}
else {
    Write-Host "INFO: Custom app variables not found; $customVariables" -ForegroundColor Yellow
}

# Load Environment Specific Variables 
$customVariables = "$CustomScriptPath/app-variables-$ShortEnvironment.ps1"

if (Test-Path -Path $customVariables) {
    Write-Host "INFO: Loading custom environment variables; $customVariables" -ForegroundColor Yellow

    . $customVariables
}
else {
    Write-Host "INFO: Custom environment variables not found; $customVariables" -ForegroundColor Yellow
}

# Load any Variable Overrides supplied on cmd line (for debugging / testing)
foreach ($variable in $VariableOverrides.Keys) {
    Write-Debug "Set Variable: $variable = $($VariableOverrides[$variable])"
    
    Set-Variable `
        -Name $variable `
        -Value $VariableOverrides[$variable] `
        -Force
}

# Validate known variables (i.e. StorageAccount resource names must be)
$storageAccountNameRegEx = "^[a-z0-9]{3,24}$"
if (${product-StorageAccountName} -cnotmatch $storageAccountNameRegEx) {
    throw "Variable 'product-StorageAccountName' must be lowercase, contain only alphanumeric characters, and be between 3 - 24 characters in length '`$(ShortLineOfBusiness)`$(ShortEnvironment)`$(ProductName)sa'. Value=${product-StorageAccountName}"
}