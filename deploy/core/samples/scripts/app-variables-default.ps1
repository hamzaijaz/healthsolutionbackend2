# Replace or override any of the core application variables found in the foundry-core/application-deployment repo here
#  https://dev.azure.com/cpu-digital-foundry/foundry-core/_git/application-deployment?path=%2Fscripts%2Fapp-variables-core.ps1

${app-DbRoleServiceUsersMember} = "${config-ApplicationName}svc_$ShortEnvironment"