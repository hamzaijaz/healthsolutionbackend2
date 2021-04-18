# MyHealthSolution.Service.sln

In order to run this project, certain things need to be set up
1. You need to have Visual Studio on your PC
2. SQL Server Management Studio

Load the project in visual studio as "MyHealthSolution.Service.sln" as entry point

Create a new Database in SQL Server Management Studio
Choose servername: (LocalDb)\MSSQLLocalDB
and Database name: my_health_solution
Another database also needs to be created for integration tests: my_health_solution_integration_tests

Run a new query, and paste script from following file: \sql\schema\002_CreatePatientTable.sql
Run this script for both my_health_solution and my_health_solution_integration_tests

Now we come towards running the function app
When you load the project in Visual Studio, do select "FunctionApp" project from top before debugging
Then start debugging

also run the front end website.

run powershell as admin
May need to do this before running powershell script: Set-ExecutionPolicy RemoteSigned
When you are done, do: Set-ExecutionPolicy Restricted

if you see an error like:

Access to the registry key
'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell' is denied. 
To change the execution policy for the default (LocalMachine) scope, 
  start Windows PowerShell with the "Run as administrator" option. 
To change the execution policy for the current user, 
  run "Set-ExecutionPolicy -Scope CurrentUser".

run command: Set-ExecutionPolicy RemoteSigned -Scope CurrentUser

After this, you will also need to run: Install-Module sqlserver
And then this: dotnet tool install --global dotnet-ef

When done, go to this directory: \src\Domain\Scaffold
then run: .\Generate.ps1
This will generate entities from database