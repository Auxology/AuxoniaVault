FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5400
EXPOSE 5400

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/Main/Main.WebApi/Main.WebApi.csproj", "src/services/Main/Main.WebApi/"]
COPY ["src/services/Shared/Shared.csproj", "src/services/Shared/"]
COPY ["src/services/Main/Main.Application/Main.Application.csproj", "src/services/Main/Main.Application/"]
COPY ["src/services/Main/Main.Domain/Main.Domain.csproj", "src/services/Main/Main.Domain/"]
COPY ["src/services/Main/Main.SharedKernel/Main.SharedKernel.csproj", "src/services/Main/Main.SharedKernel/"]
COPY ["src/services/Main/Main.Infrastructure/Main.Infrastructure.csproj", "src/services/Main/Main.Infrastructure/"]
RUN dotnet restore "src/services/Main/Main.WebApi/Main.WebApi.csproj"
COPY . .
WORKDIR "/src/src/services/Main/Main.WebApi"
RUN dotnet build "./Main.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Main.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Main.WebApi.dll"]
