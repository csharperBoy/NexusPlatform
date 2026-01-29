for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:
dotnet ef migrations add Initial_Trader_Guid --project Trader.Server.Collector.Infrastructure/Trader.Infrastructure.csproj --startup-project Trader.Server.Management.WebApi/Trader.Server.Managemen.WebApi.csproj --context <target_dbContext> --output-dir Migrations
dotnet ef migrations add <migration_name> --project Trader.Server.Collector.Infrastructure/Trader.Infrastructure.csproj --startup-project Trader.Server.Managemen.WebApi/Trader.Server.Managemen.WebApi.csproj --context <target_dbContext> --output-dir Migrations

dotnet ef migrations add Initial_Trader -p Trader.Server.Collector.Infrastructure -s "Trader.Server.Management.WebApi"
or
Add-Migration Edit_Trader1 -Project Trader.Server.Collector.Infrastructure -Context TraderDbContext -Start "Trader.Server.Management.WebApi"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Trader.Server.Collector.Infrastructure -s  "Trader.Server.Management.WebApi"
or
update-database -Project Trader.Server.Collector.Infrastructure -Context TraderDbContext -Start "Trader.Server.Management.WebApi"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Trader.Server.Collector.Infrastructure/Trader.Server.Collector.Infrastructure.csproj --startup-project Trader.Server.Management.WebApi/Trader.Server.Management.WebApi.csproj --context TraderDbContext
