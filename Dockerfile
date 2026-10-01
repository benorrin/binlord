# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/BinLord/BinLord.csproj src/BinLord/
RUN dotnet restore src/BinLord/BinLord.csproj

COPY src/BinLord/ src/BinLord/
RUN dotnet publish src/BinLord/BinLord.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# curl is only here so Docker's own HEALTHCHECK can hit /health.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# SQLite database lives on a mounted volume so it survives container recreation.
RUN mkdir -p /data && chown -R app:app /data
VOLUME /data

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__BinLordContext="Data Source=/data/binlord.db"
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER app
ENTRYPOINT ["dotnet", "BinLord.dll"]
