# alert-rule-create.ps1

> Used to create an alert rule, which captures the target and criteria for alerting. The alert rule can be in an enabled or a disabled state. Alerts only fire when enabled.

See [Alerts Overview Documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-overview)

## Parameters

This script is standalone and is usually called from other scripts.
This script can be used to create alerts for individual apps.

All parameters are listed below.

| Name                      | Description                                   | 
| ------------------------- | --------------------------------------------- | 
| TargetResourceGroupName   | The resource group containing the target resource for the alert   |
| TargetResourceName        | The name of the resource which is targeted by the alert.  |
| MetricName                | Metric Name. |
| AlertRuleSeverity         | Severity of alert. Defaults to 3 |
| AlertRuleDimensions    | An array of dimensions(object), each dimension can contain properties listed below  |
|                        | Name: Name of the Dimension |
|                        | Values: Array of string of accepted values |
|                        | Description(Optional): Description, which will be appended into the alert name |
| AlertRuleOperator   | Operator used for alert. |
| AlertRuleThreshold   | Alert threshold expressed as a timespan. |
| AlertRuleWindowSize   | Alert window size. |
| AlertRuleEvaluationFrequency   | Alert evaluation frequency to determine how often . |
| ActionGroupName   | Alert group name to associate rule with. |
| ActionGroupResourceGroupName   | Resource group containing the alert group to associate rule with. |

## Variables

No variables are overridable in this script.

## Description

Creates an alert rule with the required configuration. 

## Examples

### 1. From the app service plan script. Creating a new alert rule for availability

```powershell

$dimensions = @(
@{
Name = "availabilityResult/name"
Values = "dfdev-admin-service"
Description = "dfdev-admin-service"
},
@{
Name = "availabilityResult/success"
Values = "1"
}
)

$PSScriptRoot/alert-rule-create.ps1 `
            -TargetResourceGroupName "rg-dev-luj" `
            -TargetResourceName "PlayGround-Logs" `
            -MetricName "availabilityResults/count" `
            -AlertRuleDimensions $dimensions `
            -AlertRuleAggregationType "Count" `
            -AlertRuleOperator "LessThan" `
            -AlertRuleThreshold 1 `
            -AlertRuleWindowSize 0:5 `
            -AlertRuleEvaluationFrequency 0:1
            -ActionGroupName ${product-AlertActionGroupName} `
            -ActionGroupResourceGroupName ${product-AlertActionGroupResourceGroup}
```

