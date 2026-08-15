for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Initial_Contact -p Contact.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Edit_1_Contact -Project Contact.Infrastructure -Context ContactDbContext -Start "AkSteel.WebApi"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p Contact.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project Contact.Infrastructure -Context ContactDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project Contact.Infrastructure/Contact.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context ContactDbContext



scaffold-dbcontext "Server = localhost,1434; Database = AksteelDb; User Id = sa; Password = Fajr@123;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.sqlserver -outputdir "TempEntities"  -Context "TempDbContext"  -Project Contact.Infrastructure -Start "AkSteel.WebApi" -force

  