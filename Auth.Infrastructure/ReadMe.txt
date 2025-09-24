for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:
dotnet ef migrations add <migration_name> --project Auth.Infrastructure/Auth.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Auth.Infrastructure/Auth.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context AuthDbContext
