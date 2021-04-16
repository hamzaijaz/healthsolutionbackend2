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

Write-Host "STEP: Configuring ServiceBus" -ForegroundColor Green

$module = Get-InstalledModule -Name "Az" -ErrorAction SilentlyContinue
if ($null -eq $module) {
    Install-Module -Name "Az" -Force -AllowClobber
}

# Load Variables
. $PSScriptRoot/app-variables-core.ps1 `
                    -CustomScriptPath $CustomScriptPath `
                    -ShortEnvironment $ShortEnvironment `
                    -VariableOverrides $VariableOverrides

$serviceBusConfigFilePath = "$CustomScriptPath/servicebus-config.json"

if($ShortEnvironment -ceq "local")
{
    Write-Host "INFO: Creating ServiceBus; ${product-ServiceBusName} in ${product-ServiceBusResourceGroup}" -ForegroundColor Yellow
    az servicebus namespace create --name ${product-ServiceBusName} -g ${product-ServiceBusResourceGroup} --sku Standard
}
# Apply any additional settings to each slot
if (Test-Path -Path "$serviceBusConfigFilePath") {
    
    $configJson = Get-Content -Raw -Path "$serviceBusConfigFilePath" | ConvertFrom-Json

    # Configure Queues
    if (($null -eq $configJson.queues) -or ($configJson.queues.count -eq 0)) {
        Write-Host "INFO: No ServiceBus Queues Configured; $serviceBusConfigFilePath" -ForegroundColor Yellow
    }
    else {
        Write-Host "INFO: Configuring ServiceBus; $serviceBusConfigFilePath" -ForegroundColor Yellow
        foreach ($queue in $configJson.queues) {
            if ($queue.delete -ceq "true") {
                Write-Host "INFO: Deleting Queue; $($queue.name)" -ForegroundColor Yellow
                az servicebus queue delete --name $queue.name `
                                           --namespace-name ${product-ServiceBusName} `
                                           --resource-group ${product-ServiceBusResourceGroup}
            }
            else {
                $maxSize = if ($null -eq $queue.maxSize) { ${product-ServiceBusDefaultMaxSize} } else { $queue.maxSize }
                $maxDeliveryCount = if ($null -eq $queue.maxDeliveryCount) { ${product-ServiceBusDefaultMaxDeliveryCount} } else { $queue.maxDeliveryCount }

                Write-Debug "maxSize=$maxSize"
                Write-Debug "maxDeliveryCount=$maxDeliveryCount"
                Write-Host "INFO: Creating Queue: $($queue.name)" -ForegroundColor Yellow
                az servicebus queue create --name $queue.name `
                                           --namespace-name ${product-ServiceBusName} `
                                           --resource-group ${product-ServiceBusResourceGroup} `
                                           --max-size $maxSize `
                                           --max-delivery-count $maxDeliveryCount
            }
        }
    }

    # Configure Topics
    if (($null -eq $configJson.topics) -or ($configJson.topics.count -eq 0)) {
        Write-Host "INFO: No ServiceBus Topics Configured; $serviceBusConfigFilePath" -ForegroundColor Yellow
    }
    else {
        foreach ($topic in $configJson.topics) {
            if ($topic.delete -ceq "true") {
                Write-Host "INFO: Deleting Topic; $($topic.name)" -ForegroundColor Yellow
                az servicebus topic delete --name $topic.name `
                                           --namespace-name ${product-ServiceBusName} `
                                           --resource-group ${product-ServiceBusResourceGroup}
            }
            else {
                $maxSize = if ($null -eq $topic.maxSize) { ${product-ServiceBusDefaultMaxSize} } else { $topic.maxSize }

                Write-Host "INFO: Creating Topic; $($topic.name)" -ForegroundColor Yellow
                az servicebus topic create --name $topic.name `
                                           --namespace-name ${product-ServiceBusName} `
                                           --resource-group ${product-ServiceBusResourceGroup} `
                                           --max-size $maxSize
            }
        }
    }

    # Configure Subscriptions
    if (($null -eq $configJson.subscriptions) -or ($configJson.subscriptions.count -eq 0)) {
        Write-Host "INFO: No ServiceBus Subscriptions Configured; $serviceBusConfigFilePath" -ForegroundColor Yellow
    }
    else {
        foreach ($subscription in $configJson.subscriptions) {
            if ($subscription.delete -ceq "true") {
                Write-Host "INFO: Deleting Subscription; $($subscription.name)" -ForegroundColor Yellow
                az servicebus topic subscription delete --name $subscription.name `
                                                        --topic-name $subscription.topicName `
                                                        --namespace-name ${product-ServiceBusName} `
                                                        --resource-group ${product-ServiceBusResourceGroup}
            }
            else {
                $maxDeliveryCount = if ($null -eq $subscription.maxDeliveryCount) { ${product-ServiceBusDefaultMaxDeliveryCount} } else { $subscription.maxDeliveryCount }

                # Consumers should create the topic requried by the subscription if it doesn't exist
                # Helps with service isolation and stops deployment race conditions
                # Any additional topic config, maxSize etc will be configured when the topic owner
                # is deployed
                $topicExists = az servicebus topic list --resource-group ${product-ServiceBusResourceGroup} `
                                                   --namespace-name ${product-ServiceBusName} `
                                                   --query "[?name=='$($subscription.topicName)']" | ConvertFrom-Json

                if (!$topicExists) {
                    Write-Host "INFO: Creating Topic for Subscription; $($subscription.topicName) / $($subscription.name)" -ForegroundColor Yellow
                    az servicebus topic create --name $subscription.topicName `
                                               --namespace-name ${product-ServiceBusName} `
                                               --resource-group ${product-ServiceBusResourceGroup}
                }
                
                Write-Host "INFO: Creating Subscription; $($subscription.name)" -ForegroundColor Yellow
                az servicebus topic subscription create --name $subscription.name `
                                                        --namespace-name ${product-ServiceBusName} `
                                                        --resource-group ${product-ServiceBusResourceGroup} `
                                                        --topic-name $subscription.topicName `
                                                        --max-delivery-count $maxDeliveryCount

                if ($null -ne $subscription.sqlFilter) {
                    Write-Host "INFO: Creating Subscription SQLFilter Rule For $($subscription.name)" -ForegroundColor Yellow
                    az servicebus topic subscription rule create `
                                        --namespace-name ${product-ServiceBusName} `
                                        --resource-group ${product-ServiceBusResourceGroup} `
                                        --topic-name $subscription.topicName `
                                        --subscription-name $subscription.name `
                                        --name "SqlFilter" `
                                        --filter-sql-expression $subscription.sqlFilter
                }
            }
        }
    }
}
else {
    Write-Host "WARNING: Service Bus Config File Not Found; $serviceBusConfigFilePath" -ForegroundColor DarkRed
}
