# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY DataCollector.slnx .
COPY DataCollector.Core/DataCollector.Core.csproj DataCollector.Core/
COPY DataCollector.Infrastructure/DataCollector.Infrastructure.csproj DataCollector.Infrastructure/
COPY DataCollector.Api/DataCollector.Api.csproj DataCollector.Api/

# Restore dependencies
RUN dotnet restore DataCollector.Api/DataCollector.Api.csproj

# Copy source code
COPY DataCollector.Core/ DataCollector.Core/
COPY DataCollector.Infrastructure/ DataCollector.Infrastructure/
COPY DataCollector.Api/ DataCollector.Api/

# Build the application
RUN dotnet build DataCollector.Api/DataCollector.Api.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish DataCollector.Api/DataCollector.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000/udp

# Create data directory for SQLite database
RUN mkdir -p /data && chmod 777 /data

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DataCollector.Api.dll"]
