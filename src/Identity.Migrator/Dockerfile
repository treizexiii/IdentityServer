FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Identity.Migrator/Identity.Migrator.csproj", "src/Identity.Migrator/"]
RUN dotnet restore "src/Identity.Migrator/Identity.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Identity.Migrator"
RUN dotnet build "Identity.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Identity.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true /p:PublishSingleFile=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Identity.Migrator.dll"]
