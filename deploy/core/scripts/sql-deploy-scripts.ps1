#Requires -Version 5.0

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [string] $SqlScriptPath = "$RootPath/sql",
    [hashtable] $VariableOverrides = @{}
)

$ErrorActionPreference = "stop"

# Logging in Azure Portal
try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("DigitalFoundryDevOps-$UI$($host.name)".replace(" ", "_"), "1.0")
} 
catch { }

Write-Host "STEP: Deploying Sql Scripts" -ForegroundColor Green

# Are there SQL files to execute?
if ( -not (Test-Path -Path "$SqlScriptPath" -PathType Container) ) {
    Write-Host "INFO: Sql Scripts Not Found; $scriptPath" -ForegroundColor Yellow
    exit
}

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

if ($ShortEnvironment -cne "local") {
    if (${app-DbRoleServiceUsersMember} -ceq 'NotSet') {
        ${app-DbRoleServiceUsersMember} = "${config-ApplicationName}svc_$ShortEnvironment"
    }

    # SecureString?  Probably not worth the effort, if you have access to the kv you can see the secret anyway
    $sqlDeploymentPassword = az keyvault secret show `
                                --vault-name ${product-KeyVaultName} `
                                --name product-kv-sqlDeploymentPassword `
                                --query value

    $sqlServerConnectionString = "Server=tcp:${product-SqlServerFullName};User ID=${product-SqlServerAdminUsername};Password=$sqlDeploymentPassword;Encrypt=True;TrustServerCertificate=False;"
    $sqlDatabaseConnectionString = "Server=tcp:${product-SqlServerFullName};Initial Catalog=${app-DatabaseName};User ID=${product-SqlServerAdminUsername};Password=$sqlDeploymentPassword;Encrypt=True;TrustServerCertificate=False;"

    $dbExists = if ($(az sql db list --resource-group ${product-ResourceGroup} `
                                       --server ${product-SqlServerShortName} `
                                       --query "[?name=='${app-DatabaseName}'].databaseId" `
                                       --output tsv)) { $true } else { $false }

    # If the azure db doesn't exist, generate a password for the service account
    # we can't do this when the db exists because it will update the connection string in key vault
    # but the SQL account will still have the old password
    # therefore only generate and store the connection string if db doesn't exist
    # have connection problems? drop the db and let it re-create, data management is up to you
    if (($dbExists -eq $false) -and (${app-DbRoleServiceUsersMember} -cne 'NotSet')) {
        Add-Type -AssemblyName System.Security

        [Reflection.Assembly]::LoadWithPartialName("System.Security")
        $rijndael = new-Object System.Security.Cryptography.RijndaelManaged
        $rijndael.GenerateKey()

        ${secret-DbRoleServiceUsersPassword} = $([Convert]::ToBase64String($rijndael.Key))

        $rijndael.Dispose()

        az keyvault secret set `
                --name "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-${config-ApplicationName}-sql-connectionstring" `
                --value "Server=tcp:${product-SqlServerFullName};Initial Catalog=${app-DatabaseName};User ID=${app-DbRoleServiceUsersMember};Password=${secret-DbRoleServiceUsersPassword};Encrypt=True;TrustServerCertificate=False" `
                --vault-name ${product-KeyVaultName}
    }
}
else {
    $sqlServerConnectionString = "Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;"
    $sqlDatabaseConnectionString = "Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=${app-DatabaseName};Integrated Security=True;"
}

# Find all tokens in files
$removeComments = '^((?!\s*--).)+$'
$findTokenNames = '(?<=#\{)(.+?)(?=\}#)'

$tokens = Get-ChildItem -Path "$SqlScriptPath" -Filter '*.sql' -Recurse | `
            Select-String -Pattern $removeComments -AllMatches | `
            ForEach-Object { $_.Matches } | `
            ForEach-Object { $_.Value | Select-String -Pattern $findTokenNames -AllMatches } | `
            ForEach-Object { $_.Matches } | `
            ForEach-Object { $_.Value }

$tokens = $tokens | Select-Object -Unique

# Check that we've declared variables for all tokens in SQL files
$missingTokens = @()
$scriptVariables = @{}
foreach($token in $tokens) {
    $value = Get-Variable -Name $token -ValueOnly -ErrorAction SilentlyContinue

    Write-Debug "Token=$token; Value=$value"
    if ($null -eq $value) {
        $missingTokens += $token
    }
    else {
        $scriptVariables.Add("$token", "$value")
    }

}

if ($missingTokens.Length -gt 0) {
    throw "SQL tokens do not have values set; $($missingTokens -join ',')"
}

# Import-Module dbops
Write-Host "INFO: Checking module dependencies" -ForegroundColor Yellow

$module = Get-InstalledModule -Name "dbops" -ErrorAction SilentlyContinue
if ($null -eq $module) {
    Write-Host "INFO: Installing required module; dbops" -ForegroundColor Yellow
    Install-Module -Name "dbops" -Scope CurrentUser -Force -AllowClobber
}

$deploymentConfiguration = @{}

Set-DBODefaultSetting -Name config.variableToken -Value "\#\{(token)\}\#"

# Don't run create scripts in local environment
if ($ShortEnvironment -cne "local") {
    $scriptPath = "$SqlScriptPath/create"
    if (Test-Path -Path $scriptPath -Filter '*.sql') {
        Write-Host "INFO: Executing Database Create Scripts" -ForegroundColor Yellow
    
        Install-DboScript `
            -ScriptPath "$scriptPath\*.sql" `
            -ConnectionString "$sqlServerConnectionString" `
            -Configuration @{ Variables = $scriptVariables; } `
            -SchemaVersionTable $null
    }
    else {
        Write-Host "INFO: Skipped Database Create Scripts Not Found; $scriptPath" -ForegroundColor Yellow
    }
}
else {
    Write-Host "INFO: Skipped Database Create Scripts Not Executed Locally" -ForegroundColor Yellow

    # Ensure the database is created
    $deploymentConfiguration = @{ CreateDatabase = $true; }
}

$scriptPath = "$SqlScriptPath/configure"
if (Test-Path -Path $scriptPath -Filter '*.sql') {
    Write-Host "INFO: Executing Database Configure Scripts" -ForegroundColor Yellow
    
    Install-DboScript `
        -ScriptPath "$scriptPath\*.sql" `
        -ConnectionString "$sqlDatabaseConnectionString" `
        -Configuration ($deploymentConfiguration + @{ Variables = $scriptVariables; }) `
        -SchemaVersionTable $null
}
else {
    Write-Host "INFO: Skipped Database Configure Scripts Not Found; $scriptPath" -ForegroundColor Yellow
}

$scriptPath = "$SqlScriptPath/schema"
if (Test-Path -Path $scriptPath -Filter '*.sql') {
    Write-Host "INFO: Executing Database Schema Scripts" -ForegroundColor Yellow

    Install-DboScript `
        -ScriptPath "$scriptPath\*.sql" `
        -ConnectionString "$sqlDatabaseConnectionString" `
        -Configuration ($deploymentConfiguration + @{ Variables = $scriptVariables; }) `
        -SchemaVersionTable "${app-SqlSchemaVersionsTable}"
}
else {
    Write-Host "INFO: Skipped Database Schema Scripts Not Found; $scriptPath" -ForegroundColor Yellow
}

$scriptPath = "$SqlScriptPath/programmability"
if (Test-Path -Path $scriptPath -Filter '*.sql') {
    Write-Host "INFO: Executing Database Programmability Scripts" -ForegroundColor Yellow

    Install-DboScript `
        -ScriptPath "$scriptPath\*.sql" `
        -ConnectionString "$sqlDatabaseConnectionString" `
        -Configuration ($deploymentConfiguration + @{ Variables = $scriptVariables; }) `
        -SchemaVersionTable $null
}
else {
    Write-Host "INFO: Skipped Database Programmability Scripts Not Found; $scriptPath" -ForegroundColor Yellow
}

