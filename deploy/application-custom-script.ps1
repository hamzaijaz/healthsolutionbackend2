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

Write-Host "INFO: Creating application specific alert rules" -ForegroundColor Green

$dimensions = @(
    @{
        Name = "availabilityResult/name"
        Values = "${app-FunctionAppName}-unstable"
        Description = "${app-FunctionAppName}-unstable"
    },
    @{
        Name = "availabilityResult/success"
        Values = "1"
    }
)

. $CoreScriptFolder/alert-rule-create.ps1 `
    -TargetResourceGroupName ${product-AppInsightsResourceGroup} `
    -TargetResourceName ${product-AppInsightsName} `
    -MetricName "availabilityResults/count" `
    -AlertRuleDimensions $dimensions `
    -AlertRuleAggregationType "Count" `
    -AlertRuleOperator "LessThan" `
    -AlertRuleThreshold 1 `
    -AlertRuleWindowSize 0:5 `
    -AlertRuleEvaluationFrequency 0:1

Write-Host "INFO: Creating Service Bus specific alert rules" -ForegroundColor Green

$queues = Get-AzServiceBusQueue -ResourceGroup ${product-ServiceBusResourceGroup} -NamespaceName ${product-ServiceBusName}  `
                            | Select-Object Name `
                            | Where-Object { $_.Name -like 'myhealthsolution.*'}
                            
$topics = Get-AzServiceBusTopic -ResourceGroup ${product-ServiceBusResourceGroup} -NamespaceName ${product-ServiceBusName}  `
                            | Select-Object Name `
                            | Where-Object { $_.Name -like 'myhealthsolution.*'}

[string[]] $dimensionValues1 = $queues.Name
[string[]] $dimensionValues2 = $topics.Name
[string[]] $dimensionValues = $dimensionValues1 + $dimensionValues2

$serviceBusEntityDimensions = @(
    @{
        Name = "EntityName"
        Values = $dimensionValues
        Description = "My Health Solution $ShortEnvironment Queues-Topics"
    }
)

. $CoreScriptFolder/alert-rule-create.ps1 `
                -TargetResourceGroupName ${product-ServiceBusResourceGroup} `
                -TargetResourceName ${product-ServiceBusName} `
                -MetricName "DeadletteredMessages" `
                -AlertRuleSeverity 3 `
                -AlertRuleDimensions $serviceBusEntityDimensions `
                -AlertRuleAggregationType "Average" `
                -AlertRuleOperator "GreaterThan" `
                -AlertRuleThreshold 0 `
                -AlertRuleWindowSize 0:5 `
                -AlertRuleEvaluationFrequency 0:1 `
                -ActionGroupName ${product-AlertActionGroupName} `
                -ActionGroupResourceGroupName ${product-AlertActionGroupResourceGroup}

Write-Host "INFO: Creating Http specific alert rules" -ForegroundColor Green

. $CoreScriptFolder/alert-rule-create.ps1 `
        -TargetResourceGroupName ${product-ResourceGroup} `
        -TargetResourceName ${app-FunctionAppName} `
        -MetricName "Http4xx" `
        -AlertRuleAggregationType "Total" `
        -AlertRuleOperator "GreaterThan" `
        -AlertRuleThresholdSensitivity "Medium" `
        -AlertRuleWindowSize 0:5 `
        -AlertRuleEvaluationFrequency 0:1 `
        -ActionGroupName ${product-AlertActionGroupName} `
        -ActionGroupResourceGroupName ${product-AlertActionGroupResourceGroup}

. $CoreScriptFolder/alert-rule-create.ps1 `
    -TargetResourceGroupName ${product-ResourceGroup} `
    -TargetResourceName ${app-FunctionAppName} `
    -MetricName "Http5xx" `
    -AlertRuleAggregationType "Total" `
    -AlertRuleOperator "GreaterThan" `
    -AlertRuleThresholdSensitivity "Medium" `
    -AlertRuleWindowSize 0:5 `
    -AlertRuleEvaluationFrequency 0:1 `
    -ActionGroupName ${product-AlertActionGroupName} `
    -ActionGroupResourceGroupName ${product-AlertActionGroupResourceGroup}
    