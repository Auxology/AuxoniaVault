FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5200
EXPOSE 5200

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/Notifications/Notifications.WebApi/Notifications.WebApi.csproj", "src/services/Notifications/Notifications.WebApi/"]
COPY ["src/services/Notifications/Notifications.Domain/Notifications.Domain.csproj", "src/services/Notifications/Notifications.Domain/"]
COPY ["src/services/Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj", "src/services/Notifications/Notifications.Infrastructure/"]
RUN dotnet restore "src/services/Notifications/Notifications.WebApi/Notifications.WebApi.csproj"
COPY . .
WORKDIR "/src/src/services/Notifications/Notifications.WebApi"
RUN dotnet build "./Notifications.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notifications.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.WebApi.dll"]
