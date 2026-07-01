for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Initial_Sample -p Sample.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Initial_Sample -Project Sample.Infrastructure -Context SampleDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Sample.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Sample.Infrastructure -Context SampleDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Sample.Infrastructure/Sample.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context SampleDbContext
