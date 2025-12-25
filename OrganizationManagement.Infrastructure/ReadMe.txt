for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:

dotnet ef migrations add Initial_OrganizationManagement -p OrganizationManagement.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration Initial_OrganizationManagement -Project OrganizationManagement.Infrastructure -Context OrganizationManagementDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p OrganizationManagement.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project OrganizationManagement.Infrastructure -Context OrganizationManagementDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project OrganizationManagement.Infrastructure/OrganizationManagement.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context OrganizationManagementDbContext
