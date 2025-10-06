FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5300
EXPOSE 5300

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/Billing/Billing.WebApi/Billing.WebApi.csproj", "src/services/Billing/Billing.WebApi/"]
COPY ["src/services/Billing/Billing.Application/Billing.Application.csproj", "src/services/Billing/Billing.Application/"]
COPY ["src/services/Billing/Billing.Domain/Billing.Domain.csproj", "src/services/Billing/Billing.Domain/"]
COPY ["src/services/Billing/Billing.SharedKernel/Billing.SharedKernel.csproj", "src/services/Billing/Billing.SharedKernel/"]
COPY ["src/services/Billing/Billing.Infrastructure/Billing.Infrastructure.csproj", "src/services/Billing/Billing.Infrastructure/"]
RUN dotnet restore "src/services/Billing/Billing.WebApi/Billing.WebApi.csproj"
COPY . .
WORKDIR "/src/src/services/Billing/Billing.WebApi"
RUN dotnet build "./Billing.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Billing.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Billing.WebApi.dll"]
