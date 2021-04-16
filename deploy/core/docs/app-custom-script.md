# app-custom-script.ps1

> Used to run any custom deployment steps not included in the repo scripts

## Parameters

This script supports the [Common Script Parameters](common-script-parameters.md)

## Description

Any valid powershell (including az powershell and other 3rd party modules) or az cli commands can be included in this script to perform custom processing in the deployment pipeline.

_You are responsible for ensuring any 3rd party powershell modules are installed and loaded._
