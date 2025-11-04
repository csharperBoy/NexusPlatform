for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:
dotnet ef migrations add Initial_Auth_Guid --project Authentication.Infrastructure/Authentication.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations
dotnet ef migrations add <migration_name> --project Authentication.Infrastructure/Authentication.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations

dotnet ef migrations add Initial_Auth -p Authentication.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Initial_Auth -Project Authentication.Infrastructure -Context AuthenticationDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Authentication.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Authentication.Infrastructure -Context AuthenticationDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Authentication.Infrastructure/Authentication.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context AuthenticationDbContext
