﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Identity.Server/Identity.Server.csproj", "src/Identity.Server/"]
COPY ["src/Identity.Persistence/Identity.Persistence.csproj", "src/Identity.Persistence/"]
COPY ["src/Identity.Core/Identity.Core.csproj", "src/Identity.Core/"]
COPY ["src/Tools/Tools.TransactionsManager/Tools.TransactionsManager.csproj", "src/Tools/Tools.TransactionsManager/"]
COPY ["src/Identity.Services/Identity.Services.csproj", "src/Identity.Services/"]
COPY ["src/Identity.Wrappers/Identity.Wrappers.csproj", "src/Identity.Wrappers/"]
RUN dotnet restore "src/Identity.Server/Identity.Server.csproj"
COPY . .
WORKDIR "/src/src/Identity.Server"
RUN dotnet build "Identity.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Identity.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Server.dll"]
