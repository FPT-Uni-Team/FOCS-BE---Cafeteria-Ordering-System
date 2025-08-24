# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

# Publish NotificationService
WORKDIR /src/FOCS.NotificationService
RUN dotnet publish FOCS.NotificationService.csproj -c Release -o /app/publish

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# run background service
ENTRYPOINT ["dotnet", "FOCS.NotificationService.dll"]
