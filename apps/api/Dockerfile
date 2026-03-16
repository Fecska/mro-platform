# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + project files first (layer cache)
COPY Mro.Api.sln ./
COPY src/Mro.Api/Mro.Api.csproj                       src/Mro.Api/
COPY src/Mro.Application/Mro.Application.csproj        src/Mro.Application/
COPY src/Mro.Domain/Mro.Domain.csproj                  src/Mro.Domain/
COPY src/Mro.Infrastructure/Mro.Infrastructure.csproj  src/Mro.Infrastructure/

RUN dotnet restore

# Copy full source
COPY . .

RUN dotnet publish src/Mro.Api/Mro.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mro.Api.dll"]
