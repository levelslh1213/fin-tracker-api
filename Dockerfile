# =======================================================
# Stage 1: Base Runtime Image (ASP.NET Core 9.0 Alpine)
# =======================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8000
ENV ASPNETCORE_HTTP_PORTS=8000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# =======================================================
# Stage 2: SDK Build Image (Cache de dependências .NET)
# =======================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copia solution e projetos antes do código-fonte para maximizar cache de camadas
COPY FinTracker.sln ./
COPY src/FinTracker.Domain/FinTracker.Domain.csproj src/FinTracker.Domain/
COPY src/FinTracker.Infrastructure/FinTracker.Infrastructure.csproj src/FinTracker.Infrastructure/
COPY src/FinTracker.Api/FinTracker.Api.csproj src/FinTracker.Api/
COPY tests/FinTracker.Tests/FinTracker.Tests.csproj tests/FinTracker.Tests/

# Restaura dependências NuGet com cache otimizado
RUN dotnet restore FinTracker.sln

# Copia todo o código-fonte
COPY src/ src/
COPY tests/ tests/

# Compila a solução inteira em Release sem restaurar novamente
RUN dotnet build FinTracker.sln -c Release --no-restore

# =======================================================
# Stage 3: Publish Image
# =======================================================
FROM build AS publish
RUN dotnet publish src/FinTracker.Api/FinTracker.Api.csproj -c Release -o /app/publish --no-build

# =======================================================
# Stage 4: Final Production Image (Non-Root User)
# =======================================================
FROM base AS final
WORKDIR /app

# Cria diretório de uploads persistente e concede permissão ao usuário non-root 'app'
RUN mkdir -p /app/uploads && chown -R app:app /app

COPY --from=publish /app/publish .

# Executa com usuário não-root (UID $APP_UID / 1654 nativo do .NET 9)
USER $APP_UID

# Health Check nativo via wget (embutido no Alpine BusyBox)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8000/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "FinTracker.Api.dll"]
