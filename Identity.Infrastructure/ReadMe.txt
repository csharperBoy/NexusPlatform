for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:
dotnet ef migrations add Initial_Auth_Guid --project Identity.Infrastructure/Identity.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations
dotnet ef migrations add <migration_name> --project Identity.Infrastructure/Identity.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations

dotnet ef migrations add Initial_Auth -p Identity.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Initial_Identity -Project Identity.Infrastructure -Context IdentityDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Identity.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Identity.Infrastructure -Context IdentityDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Identity.Infrastructure/Identity.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context IdentityDbContext
