# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Project-6---Group-4---CSCN73060-SEC-1.csproj", "./"]
RUN dotnet restore "./Project-6---Group-4---CSCN73060-SEC-1.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "./Project-6---Group-4---CSCN73060-SEC-1.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Project-6---Group-4---CSCN73060-SEC-1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install wait-for-it script dependencies
USER root
RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

# Copy wait-for-db script
COPY wait-for-db.sh /app/wait-for-db.sh
RUN chmod +x /app/wait-for-db.sh

# Use wait-for-db script before starting the application
ENTRYPOINT ["/app/wait-for-db.sh", "dotnet", "Project-6---Group-4---CSCN73060-SEC-1.dll"]
