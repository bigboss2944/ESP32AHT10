# ESP32 AHT10 Data Collector

A C#/.NET-based data collection system for storing and visualizing temperature and humidity data from ESP32 AHT10 sensors using SQLite and Grafana.

## Architecture

This solution follows **SOLID principles** and clean architecture patterns:

### Projects

1. **DataCollector.Core** - Domain models and interfaces
   - Contains business logic and domain entities
   - Defines interfaces (ISensorReadingRepository, IDataParser, ISensorDataService)
   - No external dependencies

2. **DataCollector.Infrastructure** - Data access layer
   - Implements repository pattern with Entity Framework Core
   - SQLite database implementation
   - Depends only on Core project

3. **DataCollector.Api** - Application entry point
   - UDP listener background service
   - Dependency injection configuration
   - Worker service host

4. **DataCollector.Tests** - Unit tests
   - xUnit test framework
   - Moq for mocking
   - FluentAssertions for readable assertions
   - **31 tests, all passing**

### SOLID Principles Applied

- **Single Responsibility Principle (SRP)**: Each class has one responsibility
  - `SensorDataParser` - only parses sensor data
  - `SensorDataService` - only processes sensor data
  - `SensorReadingRepository` - only manages data persistence

- **Open/Closed Principle (OCP)**: Open for extension, closed for modification
  - Interface-based design allows easy extension
  - New parsers or repositories can be added without modifying existing code

- **Liskov Substitution Principle (LSP)**: Derived classes are substitutable
  - All implementations properly fulfill their interface contracts

- **Interface Segregation Principle (ISP)**: Clients depend only on what they need
  - Small, focused interfaces (IDataParser, ISensorDataService, ISensorReadingRepository)

- **Dependency Inversion Principle (DIP)**: Depend on abstractions
  - High-level modules (Api) depend on abstractions (Interfaces in Core)
  - Low-level modules (Infrastructure) implement abstractions

## Features

- ✅ Real-time UDP data collection from ESP32 sensors
- ✅ SQLite database for persistent storage
- ✅ Grafana dashboard for data visualization
- ✅ Docker containerization for easy deployment
- ✅ Comprehensive unit tests (31 tests)
- ✅ SOLID design principles
- ✅ Clean architecture
- ✅ Dependency injection
- ✅ Async/await patterns
- ✅ Logging and error handling

## Prerequisites

- Docker and Docker Compose
- ESP32 with AHT10 sensor (see `../ESP32AHT10` directory for firmware)
- .NET 8.0 SDK (for development)

## Quick Start

### 1. Build and Run with Docker

```bash
cd DataCollector
docker-compose up -d
```

This will start:
- Data Collector service (listening on UDP port 5000)
- Grafana (accessible at http://localhost:3000)

### 2. Access Grafana

1. Open http://localhost:3000 in your browser
2. Login with:
   - Username: `admin`
   - Password: `admin`
3. The "Sensor Data Dashboard" will be automatically provisioned

### 3. Configure ESP32

Update the ESP32 firmware configuration to send UDP packets to your Raspberry Pi's IP address on port 5000.

## Configuration

### Environment Variables

Configure the data collector service in `docker-compose.yml`:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Data Source=/data/sensordata.db
  - UdpListener__Port=5000
  - Logging__LogLevel__Default=Information
  - Logging__LogLevel__DataCollector=Debug
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/data/sensordata.db"
  },
  "UdpListener": {
    "Port": 5000
  }
}
```

## Data Format

The service expects UDP packets in the format:
```
temp=25.50,hum=60.00
```

Example:
```
temp=22.30,hum=55.20
temp=-10.50,hum=100.00
```

## Database Schema

### SensorReadings Table

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key (auto-increment) |
| Temperature | REAL | Temperature in Celsius |
| Humidity | REAL | Humidity percentage (0-100) |
| Timestamp | TEXT | UTC timestamp |
| DeviceId | TEXT | Device identifier (IP address) |

## Development

### Building the Solution

```bash
cd DataCollector
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

All 31 unit tests should pass:
- ✅ Parser tests (11 tests)
- ✅ Service tests (10 tests)
- ✅ Repository tests (10 tests)

### Running Locally

```bash
cd DataCollector.Api
dotnet run
```

The service will listen for UDP packets on port 5000.

## Testing the Service

### Send Test Data

You can test the service using netcat:

```bash
echo "temp=25.50,hum=60.00" | nc -u localhost 5000
echo "temp=22.30,hum=55.20" | nc -u localhost 5000
```

Or with PowerShell:
```powershell
$udpClient = New-Object System.Net.Sockets.UdpClient
$endpoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Parse("127.0.0.1"), 5000)
$bytes = [System.Text.Encoding]::ASCII.GetBytes("temp=25.50,hum=60.00")
$udpClient.Send($bytes, $bytes.Length, $endpoint)
$udpClient.Close()
```

## Grafana Dashboard

The dashboard includes:
- **Temperature Time Series** - Historical temperature data
- **Humidity Time Series** - Historical humidity data
- **Current Temperature** - Latest temperature reading
- **Current Humidity** - Latest humidity reading
- **Total Readings** - Count of all stored readings

## Volumes and Data Persistence

Data is persisted in Docker volumes:
- `sensor-data` - SQLite database
- `grafana-storage` - Grafana configuration and dashboards

To backup the database:
```bash
docker cp sensor-datacollector:/data/sensordata.db ./backup.db
```

## Troubleshooting

### Check Service Logs

```bash
docker logs sensor-datacollector
```

### Check Grafana Logs

```bash
docker logs sensor-grafana
```

### Verify Database

```bash
docker exec -it sensor-datacollector sh
ls -la /data/
```

### Test UDP Connectivity

Ensure firewall allows UDP port 5000:
```bash
# Linux
sudo ufw allow 5000/udp

# Check if service is listening
netstat -uln | grep 5000
```

## Project Structure

```
DataCollector/
├── DataCollector.Core/          # Domain models and interfaces
│   ├── Interfaces/
│   │   ├── IDataParser.cs
│   │   ├── ISensorDataService.cs
│   │   └── ISensorReadingRepository.cs
│   ├── Models/
│   │   └── SensorReading.cs
│   └── Services/
│       ├── SensorDataParser.cs
│       └── SensorDataService.cs
├── DataCollector.Infrastructure/ # Data access implementation
│   ├── Data/
│   │   └── SensorDataContext.cs
│   └── Repositories/
│       └── SensorReadingRepository.cs
├── DataCollector.Api/           # Application host
│   ├── Services/
│   │   └── UdpListenerService.cs
│   ├── Program.cs
│   └── appsettings.json
├── DataCollector.Tests/         # Unit tests
│   ├── Services/
│   │   ├── SensorDataParserTests.cs
│   │   └── SensorDataServiceTests.cs
│   └── Repositories/
│       └── SensorReadingRepositoryTests.cs
├── grafana/                     # Grafana configuration
│   └── provisioning/
│       ├── datasources/
│       │   └── sqlite.yml
│       └── dashboards/
│           ├── dashboard.yml
│           └── sensor-dashboard.json
├── Dockerfile
├── docker-compose.yml
├── .gitignore
└── README.md
```

## License

This project is part of the ESP32AHT10 repository.

## Contributing

Contributions are welcome! Please ensure:
- All unit tests pass
- Code follows SOLID principles
- New features include unit tests
- Code is well-documented
