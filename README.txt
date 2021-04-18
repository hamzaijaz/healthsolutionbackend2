# healthsolutionbackend2

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