for migration:
1- startup project on host project

2- for create magration execute this command in developer PowerShell:
dotnet ef migrations add Initial_User_Guid --project User.Infrastructure/User.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations
dotnet ef migrations add <migration_name> --project User.Infrastructure/User.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context <target_dbContext> --output-dir Migrations

dotnet ef migrations add Initial_User -p User.Infrastructure -s "AkSteel Welfare Platform"
or
Add-Migration EditPeople1 -Project People.Infrastructure -Context PeopleDbContext -Start "AkSteel Welfare Platform"
Add-Migration Edit5_People -Project People.Infrastructure -Context PeopleDbContext -Start "PhoneBook.WebApi"

3- for update database execute this command in developer powershell or package manager console 
dotnet ef database update -p User.Infrastructure -s  "AkSteel Welfare Platform"
or
update-database -Project User.Infrastructure -Context UserDbContext -Start "AkSteel Welfare Platform"

3- for update database execute this command in developer PowerShell:
dotnet ef database update --project User.Infrastructure/User.Infrastructure.csproj --startup-project MaharRayanesh.WebApi/MaharRayanesh.WebApi.csproj --context UserDbContext
