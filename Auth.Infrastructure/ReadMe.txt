for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:
dotnet ef migrations add Initial_Auth_Guid --project Auth.Infrastructure/Auth.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations
dotnet ef migrations add <migration_name> --project Auth.Infrastructure/Auth.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations

dotnet ef migrations add Initial_Auth -p Auth.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration EditAuth1 -Project Auth.Infrastructure -Context AuthDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Auth.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Auth.Infrastructure -Context AuthDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Auth.Infrastructure/Auth.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context AuthDbContext
