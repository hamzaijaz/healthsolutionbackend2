# Replace or override any of the core application variables found in the foundry-core/application-deployment repo here
#  https://dev.azure.com/cpu-digital-foundry/foundry-core/_git/application-deployment?path=%2Fscripts%2Fapp-variables-core.ps1

${product-AppServicePlanName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-asp"
${app-DbRoleServiceUsersMember} = "${config-ApplicationName}svc_$ShortEnvironment"
${app-FunctionAppRuntimeVersion} = 3
${app-FunctionAppName} = "${config-ShortLineOfBusiness}$ShortEnvironment-${config-ProductName}-ri"
${app-SqlSchemaVersionsTable} = "_SchemaVersions"

# Alerts
${product-AlertActionGroupName} = "Raise Support Team - $ShortEnvironment" 
${product-AlertActionGroupShortName} = "RaiseSupport"
${product-AlertActionGroupResourceGroup} = ${product-ResourceGroup}
