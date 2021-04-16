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

Write-Host "STEP: Binding FunctionApp To Apim; $configFile" -ForegroundColor Green

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

$configFile = "$CustomScriptPath/apim-bindings.json"

if (Test-Path -Path $configFile) {

    $bindingConfig = (Get-Content -Raw -Path "$configFile" | ConvertFrom-Json)

    if ($null -eq $bindingConfig.apis) {
        throw "Invalid 'apim-binding.json'.  Array not found 'apis'."
    }

    $apimContext = New-AzApiManagementContext `
                                -ResourceGroupName ${product-ApiManagementResourceGroup} `
                                -ServiceName ${product-ApiManagementName}

    Write-Host "INFO: Retrieve FunctionApp Resource; ${app-FunctionAppName}" -ForegroundColor Yellow
    $resourceId = (Get-AzResource -Name ${app-FunctionAppName} -ResourceGroupName ${product-ResourceGroup}).ResourceId
    $funcKeys = az rest --method post --uri "https://management.azure.com$resourceId/host/default/listKeys?api-version=2018-11-01" | ConvertFrom-Json

    Write-Host "INFO: Creating APIM Property; ${app-FunctionAppName}-key" -ForegroundColor Yellow
    New-AzApiManagementProperty `
                -Context $apimContext `
                -PropertyId "${app-FunctionAppName}-key" `
                -Name "${app-FunctionAppName}-key" `
                -Value $funcKeys.functionKeys.default `
                -Tag @("key","function","auto") `
                -Secret

    $credential = New-AzApiManagementBackendCredential `
            -Header @{"x-functions-key" = @("{{${app-FunctionAppName}-key}}")}

    Write-Host "INFO: Create APIM Backend; ${app-FunctionAppName}" -ForegroundColor Yellow
    New-AzApiManagementBackend `
                -Context  $apimContext `
                -BackendId ${app-FunctionAppName} `
                -Url "https://${app-FunctionAppName}.azurewebsites.net/api" `
                -Protocol http `
                -Title "${app-FunctionAppName}" `
                -Credential $credential `
                -Description "${app-FunctionAppName}" `
                -ResourceId "https://management.azure.com$resourceId"

    if(${app-DeploymentSlot} -ne "production")
    {
        $slotFunckeys = az rest --method post --uri "https://management.azure.com$resourceId/slots/${app-DeploymentSlot}/host/default/listKeys?api-version=2018-11-01" | ConvertFrom-Json

        Write-Host "INFO: Creating APIM Property; ${app-FunctionAppName}-${app-DeploymentSlot}-key" -ForegroundColor Yellow
        New-AzApiManagementProperty `
                -Context $apimContext `
                -Name "${app-FunctionAppName}-${app-DeploymentSlot}-key" `
                -PropertyId "${app-FunctionAppName}-${app-DeploymentSlot}-key" `
                -Value $slotFunckeys.functionKeys.default `
                -Tag @("key","function","auto") `
                -Secret
        
        $credential = New-AzApiManagementBackendCredential `
                -Header @{"x-functions-key" = @("{{${app-FunctionAppName}-${app-DeploymentSlot}-key}}")}
        
        Write-Host "INFO: Create APIM Backend; ${app-FunctionAppName}-${app-DeploymentSlot}" -ForegroundColor Yellow
        New-AzApiManagementBackend `
                -Context  $apimContext `
                -BackendId "${app-FunctionAppName}-${app-DeploymentSlot}" `
                -Url "https://${app-FunctionAppName}-${app-DeploymentSlot}.azurewebsites.net/api" `
                -Protocol http `
                -Title "${app-FunctionAppName}-${app-DeploymentSlot}" `
                -Credential $credential `
                -Description "${app-FunctionAppName}-${app-DeploymentSlot}" `
                -ResourceId "https://management.azure.com$resourceId/slots/${app-DeploymentSlot}"
    }

    if(${app-DeploymentSlot} -ne "production")
    {
        $policyFile = "$CustomScriptPath/core/policies/functionapp-apim-slots-policy.xml"
    }
    else
    {
        $policyFile = "$CustomScriptPath/core/policies/functionapp-apim-policy.xml"
    }
    $policyConfig = (Get-Content -Raw -Path "$policyFile") -replace '{#BackendName#}',${app-FunctionAppName}

    foreach($api in $bindingConfig.apis)
    {   
        $apiName = $api.Name
        $api.operations | ForEach-Object { 
            $opId = $_.name;
            Write-Host "INFO: Apply APIM Policy; Api:$apiName OperationId:$opId" -ForegroundColor Yellow
            Set-AzApiManagementPolicy -Context $apimContext `
                                    -ApiId $apiName `
                                    -OperationId $opId `
                                    -Policy $policyConfig
        }
    }
}
else {
    Write-Warning "INFO: Apim Binding Configuration Not Found; $configFile"
}



