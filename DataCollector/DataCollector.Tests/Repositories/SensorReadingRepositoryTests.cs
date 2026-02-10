using DataCollector.Core.Models;
using DataCollector.Infrastructure.Data;
using DataCollector.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DataCollector.Tests.Repositories;

public class SensorReadingRepositoryTests : IDisposable
{
    private readonly SensorDataContext _context;
    private readonly SensorReadingRepository _repository;

    public SensorReadingRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<SensorDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SensorDataContext(options);
        _repository = new SensorReadingRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidReading_StoresInDatabase()
    {
        // Arrange
        var reading = new SensorReading
        {
            Temperature = 25.5f,
            Humidity = 60.0f,
            Timestamp = DateTime.UtcNow,
            DeviceId = "device1"
        };

        // Act
        var result = await _repository.AddAsync(reading);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Temperature.Should().Be(25.5f);
        result.Humidity.Should().Be(60.0f);

        var storedReading = await _context.SensorReadings.FindAsync(result.Id);
        storedReading.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_NullReading_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task GetReadingsAsync_WithDataInRange_ReturnsFilteredReadings()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.AddHours(-5);
        var readings = new[]
        {
            new SensorReading { Temperature = 20.0f, Humidity = 50.0f, Timestamp = baseTime.AddHours(1) },
            new SensorReading { Temperature = 22.0f, Humidity = 55.0f, Timestamp = baseTime.AddHours(2) },
            new SensorReading { Temperature = 24.0f, Humidity = 60.0f, Timestamp = baseTime.AddHours(3) },
            new SensorReading { Temperature = 26.0f, Humidity = 65.0f, Timestamp = baseTime.AddHours(4) }
        };

        foreach (var reading in readings)
        {
            await _repository.AddAsync(reading);
        }

        // Act
        var result = await _repository.GetReadingsAsync(
            baseTime.AddHours(1.5),
            baseTime.AddHours(3.5));

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].Temperature.Should().Be(22.0f);
        resultList[1].Temperature.Should().Be(24.0f);
    }

    [Fact]
    public async Task GetReadingsAsync_NoDataInRange_ReturnsEmptyList()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var reading = new SensorReading
        {
            Temperature = 20.0f,
            Humidity = 50.0f,
            Timestamp = baseTime
        };
        await _repository.AddAsync(reading);

        // Act
        var result = await _repository.GetReadingsAsync(
            baseTime.AddHours(1),
            baseTime.AddHours(2));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLatestReadingAsync_WithData_ReturnsNewestReading()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var readings = new[]
        {
            new SensorReading { Temperature = 20.0f, Humidity = 50.0f, Timestamp = baseTime.AddHours(-2) },
            new SensorReading { Temperature = 22.0f, Humidity = 55.0f, Timestamp = baseTime.AddHours(-1) },
            new SensorReading { Temperature = 24.0f, Humidity = 60.0f, Timestamp = baseTime }
        };

        foreach (var reading in readings)
        {
            await _repository.AddAsync(reading);
        }

        // Act
        var result = await _repository.GetLatestReadingAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Temperature.Should().Be(24.0f);
        result.Humidity.Should().Be(60.0f);
    }

    [Fact]
    public async Task GetLatestReadingAsync_NoData_ReturnsNull()
    {
        // Act
        var result = await _repository.GetLatestReadingAsync();

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
