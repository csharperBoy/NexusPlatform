for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Initial_Contact -p Contact.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Initial_Contact -Project Contact.Infrastructure -Context ContactDbContext -Start "AkSteel.WebApi"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Contact.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Contact.Infrastructure -Context ContactDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Contact.Infrastructure/Contact.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context ContactDbContext
