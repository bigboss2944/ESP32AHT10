# SOLID Principles Implementation

This document explains how SOLID principles are implemented in the DataCollector solution.

## Overview

The DataCollector is designed with clean architecture and SOLID principles at its core. Each principle is carefully applied to create maintainable, testable, and extensible code.

## 1. Single Responsibility Principle (SRP)

**Definition**: A class should have only one reason to change.

### Examples in Our Code:

#### `SensorDataParser`
- **Single Responsibility**: Parse sensor data from string format
- **Does NOT**: Store data, validate business rules, or handle communication
```csharp
public class SensorDataParser : IDataParser
{
    public bool TryParse(string data, out float temperature, out float humidity)
    {
        // ONLY parses data - nothing else
    }
}
```

#### `SensorDataService`
- **Single Responsibility**: Coordinate between parser and repository
- **Does NOT**: Parse data itself or handle database operations
```csharp
public class SensorDataService : ISensorDataService
{
    public async Task<SensorReading?> ProcessDataAsync(string data, ...)
    {
        // Coordinates parsing and storage - orchestration only
    }
}
```

#### `SensorReadingRepository`
- **Single Responsibility**: Manage database operations for sensor readings
- **Does NOT**: Parse data or implement business logic
```csharp
public class SensorReadingRepository : ISensorReadingRepository
{
    public async Task<SensorReading> AddAsync(SensorReading reading, ...)
    {
        // ONLY handles database operations
    }
}
```

#### `UdpListenerService`
- **Single Responsibility**: Listen for UDP packets and delegate processing
- **Does NOT**: Parse or store data
```csharp
public class UdpListenerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ONLY receives UDP data and passes to service
    }
}
```

## 2. Open/Closed Principle (OCP)

**Definition**: Software entities should be open for extension but closed for modification.

### Examples in Our Code:

#### Interface-Based Design
All core functionality is defined through interfaces, allowing new implementations without modifying existing code:

```csharp
// Core interface - closed for modification
public interface IDataParser
{
    bool TryParse(string data, out float temperature, out float humidity);
}

// Can add new parsers without changing interface
public class SensorDataParser : IDataParser { }
public class JsonSensorDataParser : IDataParser { } // New implementation - no changes to existing code
```

#### Repository Pattern
New data sources can be added without changing the service layer:

```csharp
// Core interface
public interface ISensorReadingRepository { }

// Current implementation
public class SensorReadingRepository : ISensorReadingRepository { }

// Can add without changing existing code:
// - public class PostgreSqlSensorReadingRepository : ISensorReadingRepository { }
// - public class MongoDbSensorReadingRepository : ISensorReadingRepository { }
// - public class InMemorySensorReadingRepository : ISensorReadingRepository { }
```

## 3. Liskov Substitution Principle (LSP)

**Definition**: Objects of a superclass should be replaceable with objects of a subclass without breaking the application.

### Examples in Our Code:

#### Repository Implementations
Any implementation of `ISensorReadingRepository` can be substituted:

```csharp
// Service depends on interface
public class SensorDataService
{
    private readonly ISensorReadingRepository _repository;
    
    public SensorDataService(ISensorReadingRepository repository)
    {
        _repository = repository; // Any implementation works
    }
}

// All these substitutions work correctly:
services.AddScoped<ISensorReadingRepository, SensorReadingRepository>(); // SQLite
services.AddScoped<ISensorReadingRepository, InMemoryRepository>();      // In-Memory for testing
services.AddScoped<ISensorReadingRepository, PostgreSqlRepository>();    // PostgreSQL
```

#### Parser Implementations
The service works with any parser implementation:

```csharp
public class SensorDataService
{
    private readonly IDataParser _parser;
    
    public SensorDataService(..., IDataParser parser, ...)
    {
        _parser = parser; // Any parser implementation works
    }
}
```

## 4. Interface Segregation Principle (ISP)

**Definition**: Clients should not be forced to depend on interfaces they don't use.

### Examples in Our Code:

#### Focused Interfaces
Instead of one large interface, we have multiple small, focused interfaces:

❌ **Bad Example** (Not in our code):
```csharp
public interface IDataManager
{
    bool ParseData(string data, out float temp, out float hum);
    Task StoreData(SensorReading reading);
    Task<IEnumerable<SensorReading>> GetData();
    void SendNotification();
    void GenerateReport();
}
```

✅ **Good Example** (Our implementation):
```csharp
// Each interface has a single, focused purpose
public interface IDataParser
{
    bool TryParse(string data, out float temperature, out float humidity);
}

public interface ISensorReadingRepository
{
    Task<SensorReading> AddAsync(SensorReading reading, ...);
    Task<IEnumerable<SensorReading>> GetReadingsAsync(...);
    Task<SensorReading?> GetLatestReadingAsync(...);
}

public interface ISensorDataService
{
    Task<SensorReading?> ProcessDataAsync(string data, ...);
}
```

Clients only depend on what they need:
- `UdpListenerService` → only depends on `ISensorDataService`
- `SensorDataService` → only depends on `IDataParser` and `ISensorReadingRepository`
- Tests can mock only the interfaces they need

## 5. Dependency Inversion Principle (DIP)

**Definition**: High-level modules should not depend on low-level modules. Both should depend on abstractions.

### Examples in Our Code:

#### Dependency Flow

```
┌──────────────────────────────────────┐
│   High-Level Module (Api Project)    │
│                                       │
│   UdpListenerService                  │
│   depends on ↓                        │
│   ISensorDataService (abstraction)    │
└───────────────────────────────────────┘
           ↓ depends on
┌───────────────────────────────────────┐
│  Medium-Level (Core Project)          │
│                                        │
│  SensorDataService                     │
│  depends on ↓                          │
│  ISensorReadingRepository (abstraction)│
│  IDataParser (abstraction)             │
└────────────────────────────────────────┘
           ↓ implements
┌────────────────────────────────────────┐
│  Low-Level (Infrastructure Project)    │
│                                         │
│  SensorReadingRepository                │
│  implements ↑                           │
│  ISensorReadingRepository               │
└─────────────────────────────────────────┘
```

#### Configuration in Program.cs

```csharp
// High-level module (Api) configures dependencies
// but depends only on abstractions defined in Core

// Register abstractions with implementations
services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
services.AddSingleton<IDataParser, SensorDataParser>();
services.AddScoped<ISensorDataService, SensorDataService>();

// High-level services depend only on interfaces
services.AddHostedService<UdpListenerService>();
```

#### Benefits:

1. **Easy Testing**: Can swap implementations with mocks
```csharp
// In tests
var mockRepository = new Mock<ISensorReadingRepository>();
var mockParser = new Mock<IDataParser>();
var service = new SensorDataService(mockRepository.Object, mockParser.Object, logger);
```

2. **Easy Configuration**: Can change implementations without changing code
```csharp
// Switch to PostgreSQL
services.AddScoped<ISensorReadingRepository, PostgreSqlRepository>();

// Switch to JSON parser
services.AddSingleton<IDataParser, JsonSensorDataParser>();
```

3. **Decoupled Architecture**: Projects have clear dependencies
```
Api Project
  → references Core (abstractions only)
  → references Infrastructure (for DI registration only)

Infrastructure Project
  → references Core (implements abstractions)
  → has no knowledge of Api

Core Project
  → has NO dependencies
  → pure business logic and abstractions
```

## Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│                  DataCollector.Api                   │
│  ┌────────────────────────────────────────────────┐ │
│  │         UdpListenerService                      │ │
│  │  Depends on: ISensorDataService                 │ │
│  └────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
                       ↓ uses
┌─────────────────────────────────────────────────────┐
│                 DataCollector.Core                   │
│  ┌────────────────────────────────────────────────┐ │
│  │ Interfaces:                                     │ │
│  │  - ISensorDataService                           │ │
│  │  - IDataParser                                  │ │
│  │  - ISensorReadingRepository                     │ │
│  └────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────┐ │
│  │ Implementations:                                │ │
│  │  - SensorDataService                            │ │
│  │  - SensorDataParser                             │ │
│  └────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────┐ │
│  │ Models:                                         │ │
│  │  - SensorReading                                │ │
│  └────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
                       ↓ implements
┌─────────────────────────────────────────────────────┐
│           DataCollector.Infrastructure               │
│  ┌────────────────────────────────────────────────┐ │
│  │ Implementations:                                │ │
│  │  - SensorReadingRepository                      │ │
│  │  - SensorDataContext (EF Core)                  │ │
│  └────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

## Testing Benefits of SOLID

The SOLID design enables comprehensive testing:

### Unit Tests (31 tests, all passing)

1. **Parser Tests** (11 tests)
   - Test in isolation with no dependencies
   - SRP: Parser only tests parsing logic

2. **Service Tests** (10 tests)
   - Mock dependencies (repository, parser)
   - DIP: Service depends on abstractions, easy to mock

3. **Repository Tests** (10 tests)
   - Use in-memory database
   - LSP: In-memory DB substitutes for SQLite

### Example Test:
```csharp
[Fact]
public async Task ProcessDataAsync_ValidData_ReturnsStoredReading()
{
    // DIP: Mock dependencies
    var mockRepository = new Mock<ISensorReadingRepository>();
    var mockParser = new Mock<IDataParser>();
    
    // SRP: Test only service logic
    var service = new SensorDataService(mockRepository.Object, mockParser.Object, logger);
    
    // LSP: Mock implementations work like real ones
    var result = await service.ProcessDataAsync("temp=25.50,hum=60.00");
    
    Assert.NotNull(result);
}
```

## Conclusion

The DataCollector solution demonstrates practical application of all SOLID principles:

- ✅ **SRP**: Each class has one responsibility
- ✅ **OCP**: Extensible without modification
- ✅ **LSP**: Implementations are substitutable
- ✅ **ISP**: Focused, segregated interfaces
- ✅ **DIP**: Depends on abstractions, not concretions

This results in code that is:
- **Maintainable**: Easy to understand and modify
- **Testable**: 31 unit tests with 100% pass rate
- **Extensible**: New features don't require changing existing code
- **Flexible**: Easy to swap implementations
- **Decoupled**: Clear separation of concerns

## Further Reading

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
