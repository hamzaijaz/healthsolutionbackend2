#Requires -Version 5.0
#Requires -Module Az.Monitor

Param(
    [Parameter(Mandatory=$true)]
    [string] 
    $TargetResourceGroupName,
    [Parameter(Mandatory=$true)]
    [string] 
    $TargetResourceName,
    [Parameter(Mandatory=$true)]
    [string] 
    $MetricName,
    [int] 
    $AlertRuleSeverity = 3,
    #Param Dimensions{
    #     [string] Name,
    #     [string[]] Values,
    #     [string] Description
    # }
    [object[]]
    $AlertRuleDimensions,
    [string] 
    [Parameter(Mandatory=$true)]
    $AlertRuleAggregationType,
    [string] 
    [Parameter(Mandatory=$true)]
    $AlertRuleOperator,
    [Nullable[System.Int32]] 
    $AlertRuleThreshold,
    [ValidateSet("Low","Medium","High")]
    [string] 
    $AlertRuleThresholdSensitivity,
    [Parameter(Mandatory=$true)]
    [timespan] 
    $AlertRuleWindowSize,
    [Parameter(Mandatory=$true)]
    [timespan] 
    $AlertRuleEvaluationFrequency,
    [string] 
    $ActionGroupName,
    [string] 
    $ActionGroupResourceGroupName
)

if($null -eq $AlertRuleThreshold -and $AlertRuleThresholdSensitivity.Length -eq 0) 
{
    throw "AlertRuleThreshold (Static Threshold) or AlertRuleThresholdSensitivity (Dynamic Threshold) must be supplied."
}
if($null -ne $AlertRuleThreshold -and $AlertRuleThresholdSensitivity.Length -ne 0) 
{
    throw "AlertRuleThreshold (Static Threshold) and AlertRuleThresholdSensitivity (Dynamic Threshold) must not be supplied at the same time."
}

$ResourceName = $TargetResourceName

if($AlertRuleDimensions -ne '')
{
    foreach ($dim in $AlertRuleDimensions) {
        if ($dim.Description -ne $null){
            $ResourceName += " - " + $dim.Description
        }
    }
}

# Log to console
Write-Host "`n`rINFO: Creating $MetricName Alert Rule on $ResourceName..."

# Name for the alert rule
$alertRuleName = ($ResourceName + " - " + $MetricName + " Issue")
$alertRuleName = $alertRuleName.replace("/","")

# Make a description for the alert rule
$description = ("The resource " + $ResourceName + " has exceeded the set threshold.")

$targetResource = Get-AzResource -Name $TargetResourceName -ResourceGroupName $TargetResourceGroupName
if($null -eq $targetResource)
{
    throw "Resource with name $TargetResourceName could not be located in group $TargetResourceGroupName"
}

if("" -ne $ActionGroupName){
    $actionGroupResource = Get-AzResource -Name $ActionGroupName -ResourceGroupName $ActionGroupResourceGroupName
    if($null -ne $actionGroupResource){
        # Get the action group object
        $actionGroup = Get-AzActionGroup `
            -Name $ActionGroupName `
            -ResourceGroupName $ActionGroupResourceGroupName 3> $null

        # Create a new action group in local memory which will be associated to an existing action group in Azure
        $actionGroupObj = New-AzActionGroup -ActionGroupId $actionGroup.Id 3> $null
    }
    else{
        Write-Warning "WARNING: $MetricName NOT CONFIGURED on $TargetResourceName for environment $ShortEnvironment : Action Group '$ActionGroupName' is missing or not configured. Make sure you have configured a default or environment specific alert action group configuration file in the custom script folder ($SrcPath/deploy)."
        exit 1;
    }
}
else{
    $actionGroupObj = $null;
}

# This creates the condition of the Metric Alert Rule to be pass into the Add-AzMetricAlertRuleV2 cmdlet
if($null -ne $AlertRuleDimensions)
{
    $dimensions = @();
    foreach($dim in $AlertRuleDimensions){
        $dimensions += New-AzMetricAlertRuleV2DimensionSelection `
                        -DimensionName $dim.Name `
                        -ValuesToInclude $dim.Values 3> $null
    }
}
else
{
    $dimensions = $null
}

if($null -ne $AlertRuleThreshold) {
    # Static Threshold
    $condition = New-AzMetricAlertRuleV2Criteria `
                    -MetricName $MetricName `
                    -TimeAggregation $AlertRuleAggregationType `
                    -Operator $AlertRuleOperator `
                    -DimensionSelection $dimensions `
                    -Threshold $AlertRuleThreshold 3> $null
}
else {
    # Dynamic Threshold
    $condition = New-AzMetricAlertRuleV2Criteria `
                    -Dynamic -MetricName $MetricName `
                    -TimeAggregation $AlertRuleAggregationType `
                    -Operator $AlertRuleOperator `
                    -DimensionSelection $dimensions `
                    -ThresholdSensitivity $AlertRuleThresholdSensitivity 3> $null
}

$alertParams = @{
    Name = $alertRuleName
    ResourceGroupName = $TargetResourceGroupName
    Severity = $AlertRuleSeverity
    WindowSize = $AlertRuleWindowSize
    Frequency = $AlertRuleEvaluationFrequency
    TargetResourceId = $targetResource.Id
    Description = $description
    Condition = $condition
    Verbose = $null
    ErrorAction = "Stop"
}

#Optional params
if ($null -ne $actionGroupObj){
    $alertParams.ActionGroup = $actionGroupObj
}

# Create the metric alert rule for the target resource
$output = Add-AzMetricAlertRuleV2 @alertParams 3> $null

# Log to console
Write-Host "`n`rINFO: Done creating $MetricName Alert Rule on $TargetResourceName."