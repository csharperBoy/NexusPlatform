# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# کپی کردن فایل‌های csproj جهت Cache شدن دستور Restore
COPY ["AkSteel.WebApi/AkSteel.WebApi.csproj", "AkSteel.WebApi/"]
COPY ["Audit.Application/Audit.Application.csproj", "Audit.Application/"]
COPY ["Audit.Infrastructure/Audit.Infrastructure.csproj", "Audit.Infrastructure/"]
COPY ["Audit.Presentation/Audit.Presentation.csproj", "Audit.Presentation/"]
COPY ["Authorization.Application/Authorization.Application.csproj", "Authorization.Application/"]
COPY ["Authorization.Infrastructure/Authorization.Infrastructure.csproj", "Authorization.Infrastructure/"]
COPY ["Authorization.Presentation/Authorization.Presentation.csproj", "Authorization.Presentation/"]
COPY ["Base.Application/Base.Application.csproj", "Base.Application/"]
COPY ["Base.Infrastructure/Base.Infrastructure.csproj", "Base.Infrastructure/"]
COPY ["Base.Presentation/Base.Presentation.csproj", "Base.Presentation/"]
COPY ["Cach.Application/Cach.Application.csproj", "Cach.Application/"]
COPY ["Cach.Infrastructure/Cach.Infrastructure.csproj", "Cach.Infrastructure/"]
COPY ["Cach.Presentation/Cach.Presentation.csproj", "Cach.Presentation/"]
COPY ["Core.Application/Core.Application.csproj", "Core.Application/"]
COPY ["Core.Infrastructure/Core.Infrastructure.csproj", "Core.Infrastructure/"]
COPY ["Core.Presentation/Core.Presentation.csproj", "Core.Presentation/"]
COPY ["Event.Application/Event.Application.csproj", "Event.Application/"]
COPY ["Event.Infrastructure/Event.Infrastructure.csproj", "Event.Infrastructure/"]
COPY ["Event.Presentation/Event.Presentation.csproj", "Event.Presentation/"]
COPY ["HR.Application/HR.Application.csproj", "HR.Application/"]
COPY ["HR.Infrastructure/HR.Infrastructure.csproj", "HR.Infrastructure/"]
COPY ["HR.IrisaSync.Extention/HR.IrisaSync.Extention.csproj", "HR.IrisaSync.Extention/"]
COPY ["HR.Presentation/HR.Presentation.csproj", "HR.Presentation/"]
COPY ["Identity.Application/Identity.Application.csproj", "Identity.Application/"]
COPY ["Identity.Infrastructure/Identity.Infrastructure.csproj", "Identity.Infrastructure/"]
COPY ["Identity.Presentation/Identity.Presentation.csproj", "Identity.Presentation/"]
COPY ["Navigation.Application/Navigation.Application.csproj", "Navigation.Application/"]
COPY ["Navigation.Infrastructure/Navigation.Infrastructure.csproj", "Navigation.Infrastructure/"]
COPY ["Navigation.Presentation/Navigation.Presentation.csproj", "Navigation.Presentation/"]
COPY ["People.Application/People.Application.csproj", "People.Application/"]
COPY ["People.Infrastructure/People.Infrastructure.csproj", "People.Infrastructure/"]
COPY ["People.Presentation/People.Presentation.csproj", "People.Presentation/"]
COPY ["PhoneBook.Application/PhoneBook.Application.csproj", "PhoneBook.Application/"]
COPY ["PhoneBook.Infrastructure/PhoneBook.Infrastructure.csproj", "PhoneBook.Infrastructure/"]
COPY ["PhoneBook.Presentation/PhoneBook.Presentation.csproj", "PhoneBook.Presentation/"]

# بازیابی پکیج‌ها
RUN dotnet restore "AkSteel.WebApi/AkSteel.WebApi.csproj"

# کپی کامل سورس‌کد و بیلد
COPY . .
WORKDIR "/src/AkSteel.WebApi"
RUN dotnet build "AkSteel.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "AkSteel.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AkSteel.WebApi.dll"]