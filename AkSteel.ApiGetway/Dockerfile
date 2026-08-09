FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# فرض بر این است که نام پروژه ReverseProxy شما AkSteel.ReverseProxy است
COPY ["AkSteel.ReverseProxy/AkSteel.ReverseProxy.csproj", "AkSteel.ReverseProxy/"]
RUN dotnet restore "./AkSteel.ReverseProxy/AkSteel.ReverseProxy.csproj"
COPY . .
WORKDIR "/src/AkSteel.ReverseProxy"
RUN dotnet build "./AkSteel.ReverseProxy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AkSteel.ReverseProxy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AkSteel.ReverseProxy.dll"]