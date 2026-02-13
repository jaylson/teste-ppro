# =====================================================
# Partnership Manager - Backend Dockerfile
# .NET 9 Multi-stage Build
# =====================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["src/backend/PartnershipManager.API/PartnershipManager.API.csproj", "PartnershipManager.API/"]
COPY ["src/backend/PartnershipManager.Application/PartnershipManager.Application.csproj", "PartnershipManager.Application/"]
COPY ["src/backend/PartnershipManager.Infrastructure/PartnershipManager.Infrastructure.csproj", "PartnershipManager.Infrastructure/"]
COPY ["src/backend/PartnershipManager.Domain/PartnershipManager.Domain.csproj", "PartnershipManager.Domain/"]

RUN dotnet restore "PartnershipManager.API/PartnershipManager.API.csproj"

# Copiar código fonte
COPY src/backend/ .

# Build
WORKDIR "/src/PartnershipManager.API"
RUN dotnet build "PartnershipManager.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "PartnershipManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Criar usuário não-root
RUN adduser --disabled-password --gecos "" appuser

# Copiar aplicação publicada
COPY --from=publish /app/publish .

# Copiar certificado SSL do MySQL
COPY ca-certificate.pem /app/

# Criar diretório de logs e ajustar permissões
RUN mkdir -p /app/logs && chown -R appuser /app
USER appuser

# Expor porta
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "PartnershipManager.API.dll"]
