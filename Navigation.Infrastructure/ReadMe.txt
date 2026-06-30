for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Edit5_Navigation -p Navigation.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Edit_1_Navigation -Project Navigation.Infrastructure -Context NavigationDbContext -Start "TraderServer.WebApi"
for debug:
dotnet ef migrations add Edit5_Navigation --project Navigation.Infrastructure -c NavigationDbContext --verbose

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Navigation.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Navigation.Infrastructure -Context NavigationDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Navigation.Infrastructure/Navigation.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context NavigationDbContext
