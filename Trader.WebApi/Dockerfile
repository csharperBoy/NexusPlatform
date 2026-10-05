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
COPY ["Trader.WebApi/Trader.WebApi.csproj", "Trader.WebApi/"]
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
COPY ["Identity.Application/Identity.Application.csproj", "Identity.Application/"]
COPY ["Identity.Infrastructure/Identity.Infrastructure.csproj", "Identity.Infrastructure/"]
COPY ["Identity.Presentation/Identity.Presentation.csproj", "Identity.Presentation/"]
COPY ["Navigation.Application/Navigation.Application.csproj", "Navigation.Application/"]
COPY ["Navigation.Infrastructure/Navigation.Infrastructure.csproj", "Navigation.Infrastructure/"]
COPY ["Navigation.Presentation/Navigation.Presentation.csproj", "Navigation.Presentation/"]

COPY ["Scheduler.Application/Scheduler.Application.csproj", "Scheduler.Application/"]
COPY ["Scheduler.Infrastructure/Scheduler.Infrastructure.csproj", "Scheduler.Infrastructure/"]
COPY ["Scheduler.Presentation/Scheduler.Presentation.csproj", "Scheduler.Presentation/"]
COPY ["StreamBus.Application/StreamBus.Application.csproj", "StreamBus.Application/"]
COPY ["StreamBus.Infrastructure/StreamBus.Infrastructure.csproj", "StreamBus.Infrastructure/"]
COPY ["StreamBus.Presentation/StreamBus.Presentation.csproj", "StreamBus.Presentation/"]
COPY ["Trader.Application/Trader.Application.csproj", "Trader.Application/"]
COPY ["Trader.Infrastructure/Trader.Infrastructure.csproj", "Trader.Infrastructure/"]
COPY ["Trader.Presentation/Trader.Presentation.csproj", "Trader.Presentation/"]

# بازیابی پکیج‌ها
RUN dotnet restore "Trader.WebApi/Trader.WebApi.csproj"

# کپی کامل سورس‌کد و بیلد
COPY . .
WORKDIR "/src/Trader.WebApi"
RUN dotnet build "Trader.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "Trader.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Trader.WebApi.dll"]