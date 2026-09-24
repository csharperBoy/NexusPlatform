for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Initial_Scheduler -p Scheduler.Infrastructure -s "Trader Application"
or
Add-Migration Initial_Scheduler -Project Scheduler.Infrastructure -Context SchedulerDbContext -Start "Trader Application"
Add-Migration Edit_1_Scheduler -Project Scheduler.Infrastructure -Context SchedulerDbContext -Start "Trader.WebApi"

Remove-Migration -Project Scheduler.Infrastructure -Context SchedulerDbContext -Start "Trader.WebApi"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Scheduler.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Scheduler.Infrastructure -Context SchedulerDbContext -Start "Trader Applicatin"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Scheduler.Infrastructure/Scheduler.Infrastructure.csproj --startup-project Trader.WebApi/Trader.WebApi.csproj --context SchedulerDbContext
