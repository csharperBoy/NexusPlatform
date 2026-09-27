for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Initial_Trader -p Trader.Infrastructure -s "Trader Application"
or
Add-Migration Initial_Trader -Project Trader.Infrastructure -Context TraderDbContext -Start "Trader Application"
Add-Migration Edit_1_Trader -Project Trader.Infrastructure -Context TraderDbContext -Start "Trader.WebApi"

Remove-Migration -Project Trader.Infrastructure -Context TraderDbContext -Start "Trader.WebApi"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Trader.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Trader.Infrastructure -Context TraderDbContext -Start "Trader Applicatin"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Trader.Infrastructure/Trader.Infrastructure.csproj --startup-project Trader.WebApi/Trader.WebApi.csproj --context TraderDbContext
