FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5100
EXPOSE 5101

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/Auth/Auth.WebApi/Auth.WebApi.csproj", "src/services/Auth/Auth.WebApi/"]
COPY ["src/services/Auth/Auth.Application/Auth.Application.csproj", "src/services/Auth/Auth.Application/"]
COPY ["src/services/Auth/Auth.Domain/Auth.Domain.csproj", "src/services/Auth/Auth.Domain/"]
COPY ["src/services/Auth/Auth.SharedKernel/Auth.SharedKernel.csproj", "src/services/Auth/Auth.SharedKernel/"]
COPY ["src/services/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj", "src/services/Auth/Auth.Infrastructure/"]
RUN dotnet restore "src/services/Auth/Auth.WebApi/Auth.WebApi.csproj"
COPY . .
WORKDIR "/src/src/services/Auth/Auth.WebApi"
RUN dotnet build "./Auth.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.WebApi.dll"]
