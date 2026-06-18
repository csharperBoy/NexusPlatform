for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Initial_HR -p HR.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Initial_HR -Project HR.Infrastructure -Context HRDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p HR.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project HR.Infrastructure -Context HRDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project HR.Infrastructure/HR.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context HRDbContext
