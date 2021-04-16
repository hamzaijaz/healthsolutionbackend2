# servicebus-configure.ps1

> Used to configure Service Bus queues, topics, and subscriptions

## Parameters

This script supports the [Common Script Parameters](common-script-parameters.md)

## Variables

The following variables are used in this script. The values are defined in the core variables but can be overridden at the application or environment level, see [app-variables-core](app-variables-core.md) for more information.

| Name                                | Default Value                                             | Description                                    |
| ----------------------------------- | ----------------------------------------------------------| -----------------------------------------------|
| \${product-ServiceBusName}          | ${config-ShortLineOfBusiness}$ShortEnvironment-servicebus | the service bus name                           |
| \${product-ServiceBusResourceGroup} | rg-${config-ShortLineOfBusiness}$ShortEnvironment-common  | this service bus resource used for deployments |

## Description

The script reads a list of queues, topics, and subscriptions from _\$RootPath/deploy/servicebus-config.json_ which will be created or deleted from the specified service bus instance.

## Examples

The examples below can be combined into a single json configuration file to create or delete any combination of queues, topics, and subscriptions

### Queues

#### 1. Create a Queue with default configuration

The default values for queue creation are;

- maxSize: 1024
- deliveryCount: 10

_servicebus-config.json_

```json
{
  "queues": [
    {
      "name": "my.sample.queue"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

#### 2. Create a Queue with configuration overrides

_servicebus-config.json_

```json
{
  "queues": [
    {
      "name": "my.sample.queue",
      "maxSize": "2048",
      "deliveryCount": "1"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

#### 3. Delete a Queue

_servicebus-config.json_

```json
{
  "queues": [
    {
      "name": "my.sample.queue",
      "delete": "true"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

### Topics

#### 1. Create a Topic with default configuration

The default values for topic creation are;

- maxSize: 1024

_servicebus-config.json_

```json
{
  "topics": [
    {
      "name": "my.sample.topic"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

#### 2. Create a Topic with configuration overrides

_servicebus-config.json_

```json
{
  "topics": [
    {
      "name": "my.sample.topic",
      "maxSize": "2048"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

#### 3. Delete a Topic

_servicebus-config.json_

```json
{
  "topics": [
    {
      "name": "my.sample.topic",
      "delete": "true"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

### Subscriptions

#### 1. Create a Subscription with default configuration

The default values for subscription creation are;

- maxDeliveryCount: 10
- sqlFilter: none

_servicebus-config.json_

```json
{
  "subscriptions": [
    {
      "name": "my.subscription",
      "topicName": "my.sample.topic"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

#### 2. Create a Subscription with configuration overrides

_servicebus-config.json_

```json
{
  "subscriptions": [
    {
      "name": "my.subscription",
      "topicName": "my.sample.topic",
      "maxDeliveryCount": "1",
      "sqlFilter": "FieldA='Apple'"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```

#### 3. Delete a Subscription

_servicebus-config.json_

```json
{
  "subscriptions": [
    {
      "name": "my.subscription",
      "topicName": "my.sample.topic",
      "delete": "true"
    }
  ]
}
```

```powershell
.\deploy\core\scripts\servicebus-configure.ps1
```
