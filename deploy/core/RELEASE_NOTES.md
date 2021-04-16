# Release Notes

View latest [release notes](https://dev.azure.com/cpu-digital-foundry/foundry-core/_git/application-deployment?path=%2FRELEASE_NOTES.md&_a=preview).

## 1.0.8 (2020.08.11)

### Work Items

[6690 | application-deployment | stop slot after swap](https://dev.azure.com/cpu-digital-foundry/foundry-core/_workitems/edit/6690/)

> Stop source slot after a successful swap to avoid old app versions processing messages from service bus triggers

## 1.0.7 (2020.07.07)

### Work Items

[5948 | application-deployment | add service bus subscription filters](https://dev.azure.com/cpu-digital-foundry/foundry-core/_workitems/edit/5948/)

> Allow service bus subscriptions to define sql filters

## 1.0.6 (2020.07.02)

### Work Items

[5858 | application-deployment | add upgrade docs](https://dev.azure.com/cpu-digital-foundry/foundry-core/_workitems/edit/5858/)

> Updated README to include a section on how to upgrade the core submodule to a later version.

## 1.0.5 (2020.06.26)

### Work Items

[5362 | application-deployment: creating missing service bus topic on subscription create](https://dev.azure.com/cpu-digital-foundry/foundry-core/_workitems/edit/5362/)

> To help with the deployment separation of services where a consumer MUST be deployed after the producer when subscribing to a topic, the script needs to be updated to create a missing topic when creating a subscription.  When the producer deployment runs, it will apply any additional configuration to the topic.  Doing this removes the deployment dependency between producer and consumer services.
