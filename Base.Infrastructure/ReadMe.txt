for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Edit5_Base -p Base.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Edit_1_Base -Project Base.Infrastructure -Context BaseDbContext -Start "TraderServer.WebApi"
for debug:
dotnet ef migrations add Edit5_Base --project Base.Infrastructure -c BaseDbContext --verbose

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Base.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Base.Infrastructure -Context BaseDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Base.Infrastructure/Base.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context BaseDbContext
