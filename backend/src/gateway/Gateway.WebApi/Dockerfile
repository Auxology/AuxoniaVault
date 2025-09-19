FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/gateway/Gateway.WebApi/Gateway.WebApi.csproj", "src/gateway/Gateway.WebApi/"]
RUN dotnet restore "src/gateway/Gateway.WebApi/Gateway.WebApi.csproj"
COPY . .
WORKDIR "/src/src/gateway/Gateway.WebApi"
RUN dotnet build "./Gateway.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gateway.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.WebApi.dll"]
