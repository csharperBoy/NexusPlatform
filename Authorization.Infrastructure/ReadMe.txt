for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Edit5_Authorization -p Authorization.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Edit5_Authorization -Project Authorization.Infrastructure -Context AuthorizationDbContext -Start "AkSteel Welfare Platform"
for debug:
dotnet ef migrations add Edit5_Authorization --project Authorization.Infrastructure -c AuthorizationDbContext --verbose

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Authorization.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Authorization.Infrastructure -Context AuthorizationDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Authorization.Infrastructure/Authorization.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context AuthorizationDbContext
