#Requires -Version 5.0

Param(
    [string] $ShortEnvironment = "local",
    [string] $RootPath = ".",
    [string] $CustomScriptPath = "$RootPath/deploy",
    [string] $SqlScriptPath = "$RootPath/sql",
    [string] $ArmTemplatePath = "$RootPath/deploy/core/arm",
    [string] $FunctionAppZipFilePath = "$RootPath/functionapp/service.zip",
    [hashtable] $VariableOverrides = @{}
)

$ErrorActionPreference = "stop"

try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("DigitalFoundryAppGitOps-$UI$($host.name)".replace(" ", "_"), "1.0")
} 
catch { }

Write-Host "STEP: Running Application Pipeline" -ForegroundColor Green

# Make sure Az powershell module is installed
Write-Host "INFO: Checking module dependencies" -ForegroundColor Yellow
$module = Get-InstalledModule -Name "Az" -RequiredVersion 3.7.0 -ErrorAction SilentlyContinue
if ($null -eq $module) {
    Write-Host "INFO: Installing required module; Az" -ForegroundColor Yellow
    Install-Module -Name "Az" -Force -AllowClobber -RequiredVersion 3.7.0
}

# If this script is called from a DevOps pipeline using an AzureCLI task
# then we need to install the Az powershell module.
# This lets us use Az Cli and Az Powershell together in a single script of awesomeness
# Using the 'addSpnToEnvironment' input on the AzureCLI pipeline task
# provides the service principal details as variables (securestrings)
if (($null -ne $env:servicePrincipalId) -and 
    ($null -ne $env:servicePrincipalKey) -and 
    ($null -ne $env:tenantId)) {
    Write-Host "INFO: Running in a pipeline." -ForegroundColor Yellow
    Write-Host "INFO: Checking module dependencies" -ForegroundColor Yellow

    Write-Host "INFO: Authenticating Az Powershell with Service Principal..." -ForegroundColor Yellow

    $spnPassword = ConvertTo-SecureString $env:servicePrincipalKey -AsPlainText -Force
    $credential = New-Object System.Management.Automation.PSCredential -ArgumentList ($env:servicePrincipalId, $spnPassword)
    Connect-AzAccount -Credential $credential -Tenant $env:tenantId -ServicePrincipal
    
    # The Service Principal may have a default subscription different to the once used in pipelines' ServiceConnection
    # get the current subscription name based on the current az cli context (from the ServiceConnection)
    $subscriptionName = az account show --query name --output tsv

    Write-Host "DEBUG: Service Connection Subscription = '$subscriptionName'" -ForegroundColor Cyan
    $psContext = Get-AzContext
    Write-Host "DEBUG: Current Powershell Context Subscription = '$($psContext.Subscription.Name)'" -ForegroundColor Cyan

    # Set the powershell context to use the same subscription as the ServiceConnection
    Write-Host "DEBUG: Set PowerShell Context Subscription = '$subscriptionName'" -ForegroundColor Cyan
    Set-AzContext -Subscription $subscriptionName
    $psContext = Get-AzContext
    Write-Host "DEBUG: Current Powershell Context Subscription = '$($psContext.Subscription.Name)'" -ForegroundColor Cyan

}

$arguments = @{
    ShortEnvironment = $ShortEnvironment;
    RootPath = $RootPath;
    CustomScriptPath = $CustomScriptPath;
    VariableOverrides = $VariableOverrides;
}

# Create FunctionApp
Invoke-Expression "& `"$PSScriptRoot\functionapp-create.ps1`" @arguments"

# Create and configure the function app deployment slot
Invoke-Expression "& `"$PSScriptRoot\functionapp-create-slot.ps1`" @arguments"

#
# Deploy SQL Scripts
# - Uses dbops (a DbUp wrapper)
# - Executes scripts in folders in name order; 
#     Create, Configure, Schema , and Programmability 
#
Invoke-Expression "& `"$PSScriptRoot\sql-deploy-scripts.ps1`" @arguments -SqlScriptPath $SqlScriptPath"

# Apply any additional settings to each slot
Invoke-Expression "& `"$PSScriptRoot\functionapp-configure-settings.ps1`" @arguments"

#
# Service Bus Topics / Queues
# - Topics, Queues, and Subscriptions are defined in
#   $CustomScriptPath/servicebus-config.json
#
Invoke-Expression "& `"$PSScriptRoot\servicebus-configure.ps1`" @arguments"

#
# Deploy Latest FunctionApp Build
# 
Invoke-Expression "& `"$PSScriptRoot\functionapp-deploy.ps1`" @arguments -FunctionAppZipFilePath $FunctionAppZipFilePath"

#
# APIM Binding
# - Binds APIM operations to the FunctionApp based on the configuration in 
#   $CustomScriptPath/apim-bindings.json
# - Configures bacend policies to support slots if required
#
Invoke-Expression "& `"$PSScriptRoot\functionapp-bind-apim.ps1`" @arguments -ArmTemplatePath $ArmTemplatePath"

#
# Execute additional product specific script
# - This can be used to create and configure any additional product specific infrastructure
# - e.g.; additional App Service Plan
#
Write-Host "INFO: Executing Custom Script" -ForegroundColor Green

$arguments += @{ CoreScriptFolder = $PSScriptRoot; }

$customScript = "$CustomScriptPath\application-custom-script.ps1"

if (Test-Path -Path $customScript) {
    Write-Host "Executing custom script from : $customScript"

    # Product Specific 
    Invoke-Expression "& `"$customScript`" @arguments"
}
else {
    Write-Warning "STEP IGNORED: Custom script not found. $customScript"
}
