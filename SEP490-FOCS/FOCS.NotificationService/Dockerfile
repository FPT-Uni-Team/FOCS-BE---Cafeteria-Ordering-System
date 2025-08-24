# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj trước (tăng cache hiệu quả và chắc chắn path đúng)
COPY FOCS.NotificationService/FOCS.NotificationService.csproj FOCS.NotificationService/
RUN dotnet restore FOCS.NotificationService/FOCS.NotificationService.csproj

# copy toàn bộ source
COPY . .

# publish
WORKDIR /src/FOCS.NotificationService
RUN dotnet publish FOCS.NotificationService.csproj -c Release -o /app/publish

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FOCS.NotificationService.dll"]
