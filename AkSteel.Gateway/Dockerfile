FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# نام پوشه و فایل پروژه پروکسی خود را در صورت تفاوت جایگزین کنید
COPY ["AkSteel.Gateway/AkSteel.Gateway.csproj", "AkSteel.Gateway/"]
RUN dotnet restore "./AkSteel.Gateway/AkSteel.Gateway.csproj"

COPY . .
WORKDIR "/src/AkSteel.Gateway"
RUN dotnet build "./AkSteel.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AkSteel.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AkSteel.Gateway.dll"]