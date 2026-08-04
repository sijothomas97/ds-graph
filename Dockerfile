# syntax=docker/dockerfile:1

# ---- Build stage -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only project files first for better layer caching on restore.
COPY SocialGraph.sln ./
COPY SocialGraph.Core/SocialGraph.Core.csproj SocialGraph.Core/
COPY SocialGraph.Tests/SocialGraph.Tests.csproj SocialGraph.Tests/
COPY SocialGraph.Api/SocialGraph.Api.csproj SocialGraph.Api/
RUN dotnet restore SocialGraph.sln

# Now copy everything else and publish just the API.
COPY SocialGraph.Core/ SocialGraph.Core/
COPY SocialGraph.Tests/ SocialGraph.Tests/
COPY SocialGraph.Api/ SocialGraph.Api/
RUN dotnet publish SocialGraph.Api/SocialGraph.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build --chown=app:app /app/publish .

# Pre-create the data directory (owned by the non-root "app" user) so the
# app can write graph.json without needing root or a pre-mounted volume.
RUN mkdir -p /app/data && chown app:app /app/data

# Run as the built-in non-root "app" user.
USER app

ENV ASPNETCORE_URLS=http://+:8080
# Persisted graph JSON lives here; mount a volume at /app/data to keep
# it across container restarts/redeploys.
ENV GRAPH_DATA_PATH=/app/data/graph.json
EXPOSE 8080

ENTRYPOINT ["dotnet", "SocialGraph.Api.dll"]
