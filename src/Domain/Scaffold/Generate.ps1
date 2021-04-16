$ApplicationName="RightsIssues"
$DatabaseName="my_health_solution"
$ConnectionString="Server=(LocalDB)\MSSQLLocalDB;Database=$($DatabaseName);Trusted_Connection=True;"
# Get a list of tables from the database, exclude the schema versions table or any other table with underscore.
# use this list to generate arguments to pass to the ef scaffold command
$Tables = Invoke-Sqlcmd -Query "SELECT TABLE_NAME FROM information_schema.TABLES Where NOT TABLE_NAME LIKE '%[_]%' ORDER BY TABLE_NAME" -ConnectionString $ConnectionString
foreach($table in $Tables)
{
    $tableCommands += " --table " + $table.TABLE_NAME
}
# remove existing files before we generate the code
if(Test-Path ..\Entities)
{
    Remove-Item ..\Entities\*.*
}

$cmd = "dotnet ef dbcontext scaffold `"$ConnectionString`" Microsoft.EntityFrameworkCore.SqlServer -c ApplicationDbContext -o ..\Entities -f $tableCommands"
Invoke-Expression $cmd

Set-Location ..\Entities

# Move the generated DbContext class into the infrastructure project
Move-Item ApplicationDbContext.cs ..\..\Infrastructure\Persistence\ApplicationDbContextGenerated.cs -force 
# For the entity classes fix the namespace
Get-ChildItem -Recurse | ForEach { (Get-Content $_.PSPath | ForEach {$_ -creplace "EFCoreScaffold", "CapitalRaising.$($ApplicationName).Service.Domain.Entities"}) | Set-Content $_.PSPath }
Set-Location ..\..\Infrastructure\Persistence

# Fix the namespaces and remove warnings from the DbContext file 
$dbContext = Get-Content -Raw -Path ApplicationDbContextGenerated.cs
$dbContext = $dbContext -creplace "EFCoreScaffold", "CapitalRaising.$($ApplicationName).Service.Infrastructure.Persistence"
$dbContext = $dbContext -creplace "using System;", "using System;`r`nusing CapitalRaising.$($ApplicationName).Service.Domain.Entities;"
$dbContext = $dbContext -creplace "#warning" , "//#warning"

Set-Content -Path ApplicationDbContextGenerated.cs -Value $dbContext -Force

Set-Location ..\..\Domain\Scaffold
