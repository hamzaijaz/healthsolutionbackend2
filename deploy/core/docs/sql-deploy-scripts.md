# sql-deploy-scripts.ps1

> Used to execute SQL scripts against a local or Azure PaaS database. The script will create the database if it doesn't exist.

## Parameters

In addition to the [Common Script Parameters](common-script-parameters.md), this script also supports the following additional parameters;

| Parameter Name      | Description                                                                                                      |
| ------------------- | ---------------------------------------------------------------------------------------------------------------- |
| \$SqlScriptPath     | The path to the folder containing custom deployment steps and variable overrides. Defaults to _'\$RootPath/sql'_ |

## Description

The database name is derived from the script [Common Script Parameters](common-script-parameters.md) values;

```powershell
$(${config-ProductCode})_$(${config-ApplicationName})_$ShortEnvironment
```

### Getting Started

You will find default Create and Configure scripts in the 'samples' folder.  Copying the 'sql' folder from 'samples' to the root of your repository will allow you to run the examples below locally to create an empty database.

### Script Locations

The script relies on specific folders under the _\$SqlScriptPath_ and executes all scripts in the folders in sequence. Files in each folder are executed in filename order.

| #   | Folder Name     | Scripts Will Run | Description                                                       |
| --- | --------------- | ---------------- | -------------------------------------------------------------- |
| 1   | Create          | Always           | The sample, recommended script will create an Azure SQL PaaS database attached to a SQL ElasticPool.  **DOES NOT EXECUTE IN A LOCAL DEV ENVIRONMENT**  |
| 2   | Configure       | Always           | The sample, recommended script creates ServiceUsers and TestServiceUsers roles with SELECT, INSERT, UPDATE, and DELETE permissions to all tables, and a SupportUsers role with SELECT permission to all tables.  The script uses variables to control role membership, see the Role Members table below for more information. |
| 3   | Schema          | Once             | The location for all your schema scripts and any other scripts that should only execute **once** (e.g. data loads).  The scripts will run in alphabetical order.  To ensure a consistent order it is recommended to prefix the name of your script with the current date (yyyyMMdd).  If multiple scripts are created on the same day, a integer counter should be used to specify the execution order; 20200519.01 Create Table.sql, 20200519.02 Load Table Data.sql |
| 4   | Programmability | Always           | the location for your Stored Procedures, Functions, etc.  Scripts in this folder will also be executed in alphabetical order. |

### Role Membership

Database role membership is controlled by the variables below.  These can be configured per environment by defining values in the respective [app variables](app-variables-core.md) file.

In a local development environment, all of these variables should be set to 'NotSet' which skips the creation of users and role membership.  In a local development the engineer should be the db_owner and using integrated authentication eliminating the need for role membership.

#### Roles

| Role Name | Permissions |
| --------- | ----------- |
| ServiceUsers| SELECT, INSERT, UPDATE, and DELETE to all tables |
| TestServiceUsers| SELECT, INSERT, UPDATE, and DELETE to all tables |
| SupportUsers| SELECT to all tables |

#### Role Members

| Variable Name | Default Value | Description |
| ------------- | ------------- | ----------- |
| ${app-DbRoleServiceUsersMember} | ${config-ApplicationName}svc_$ShortEnvironment (e.g. sampleappsvc_dev) | Provides membership to the ServiceUsers role.  This is the SQL login used to connect to the SQL database.  Although Integrated Authentication is preferred, SQL logins are currently used for Function App connection.  This may be resolved in a future release. |
| ${app-DbRoleTestServiceUsersMember} | NotSet | Provides membership to the TestServiceUsers role.  Currently cannot be used until Integrated Authentication is resolved. |
| ${app-DbRoleSupportUsersMember} | NotSet | Provides membership to the SupportUsers role.  Currently cannot be used until Integrated Authentication is resolved. |

## Examples

From the root of your application repository assuming the scripts have been added in the _'default'_ location.

### 1. Run SQL scripts

```powershell
.\deploy\core\scripts\sql-deploy-scripts.ps1
```

### 2. Run scripts against a local SQL instance with scripts in a non-standard folder

```powershell
.\deploy\core\scripts\sql-deploy-scripts.ps1 -SqlScriptPath {Path}
```
