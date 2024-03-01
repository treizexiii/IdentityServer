FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update && apt-get install -y libgdiplus libc6-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Identity.Migrator/Identity.Migrator.csproj", "src/Identity.Migrator/"]
COPY ["src/Identity.Persistence/Identity.Persistence.csproj", "src/Identity.Persistence/"]
COPY ["src/Identity.Core/Identity.Core.csproj", "src/Identity.Core/"]
COPY ["src/Tools/Tools.TransactionsManager/Tools.TransactionsManager.csproj", "src/Tools/Tools.TransactionsManager/"]
COPY ["src/Identity.Services/Identity.Services.csproj", "src/Identity.Services/"]
COPY ["src/Identity.Wrappers/Identity.Wrappers.csproj", "src/Identity.Wrappers/"]
RUN dotnet restore "src/Identity.Migrator/Identity.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Identity.Migrator"
RUN dotnet build "Identity.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Identity.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true /p:PublishSingleFile=true /p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#ENTRYPOINT ["./Identity.Migrator", "1"]
