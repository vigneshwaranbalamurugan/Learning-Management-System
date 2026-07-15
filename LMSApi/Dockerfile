# ─── Stage 1: Build ───────────────────────────────────────────────────────────
# Use the full .NET 10 SDK image to restore, build, and publish.
# Context: run `docker build` from the LMSApi/ directory.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project files first (layer-cache friendly)
COPY LMSApi.API/LMSApi.API.csproj                       LMSApi.API/
COPY LMSApi.BALLibrary/LMSApi.BALLibrary.csproj         LMSApi.BALLibrary/
COPY LMSApi.DALLibrary/LMSApi.DALLibrary.csproj         LMSApi.DALLibrary/
COPY LMSApi.ModelLibrary/LMSApi.ModelLibrary.csproj     LMSApi.ModelLibrary/

# Restore NuGet packages for the API project (automatically restores dependencies)
RUN dotnet restore LMSApi.API/LMSApi.API.csproj

# Copy remaining source code
COPY . .

# Publish the API project in Release mode to /app/publish
RUN dotnet publish LMSApi.API/LMSApi.API.csproj \
      --configuration Release \
      --output /app/publish

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
# Use the minimal ASP.NET 10 runtime image (no SDK overhead).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# .NET 8+ images have a built-in non-root user named 'app'
# We don't need to manually create one.

# Copy only the published output from the build stage
COPY --from=build /app/publish .

# Ensure the logs directory exists and is writable by the built-in 'app' user
RUN mkdir -p /app/logs && chown -R app:app /app

USER app

# ASP.NET Core listens on port 8080 by default in containers
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "LMSApi.API.dll"]
