for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:
dotnet ef migrations add Initial_Audit_Guid --project Audit.Infrastructure/Audit.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations
dotnet ef migrations add <migration_name> --project Audit.Infrastructure/Audit.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations

dotnet ef migrations add Initial_Audit -p Audit.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Edit_Audit1 -Project Audit.Infrastructure -Context AuditDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Audit.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Audit.Infrastructure -Context AuditDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Audit.Infrastructure/Audit.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context AuditDbContext
