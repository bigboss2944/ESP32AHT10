using DataCollector.Core.Interfaces;
using DataCollector.Core.Models;
using DataCollector.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DataCollector.Tests.Services;

public class SensorDataServiceTests
{
    private readonly Mock<ISensorReadingRepository> _repositoryMock;
    private readonly Mock<IDataParser> _parserMock;
    private readonly Mock<ILogger<SensorDataService>> _loggerMock;
    private readonly SensorDataService _service;

    public SensorDataServiceTests()
    {
        _repositoryMock = new Mock<ISensorReadingRepository>();
        _parserMock = new Mock<IDataParser>();
        _loggerMock = new Mock<ILogger<SensorDataService>>();
        _service = new SensorDataService(_repositoryMock.Object, _parserMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessDataAsync_ValidData_ReturnsStoredReading()
    {
        // Arrange
        var data = "temp=25.50,hum=60.00";
        var deviceId = "device1";
        var temperature = 25.50f;
        var humidity = 60.00f;

        _parserMock.Setup(p => p.TryParse(data, out temperature, out humidity))
            .Returns(true);

        var expectedReading = new SensorReading
        {
            Id = 1,
            Temperature = temperature,
            Humidity = humidity,
            DeviceId = deviceId,
            Timestamp = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SensorReading>(), default))
            .ReturnsAsync(expectedReading);

        // Act
        var result = await _service.ProcessDataAsync(data, deviceId);

        // Assert
        result.Should().NotBeNull();
        result!.Temperature.Should().Be(temperature);
        result.Humidity.Should().Be(humidity);
        result.DeviceId.Should().Be(deviceId);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SensorReading>(), default), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ProcessDataAsync_EmptyOrNullData_ReturnsNull(string? data)
    {
        // Act
        var result = await _service.ProcessDataAsync(data!);

        // Assert
        result.Should().BeNull();
        _parserMock.Verify(p => p.TryParse(It.IsAny<string>(), out It.Ref<float>.IsAny, out It.Ref<float>.IsAny), Times.Never);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SensorReading>(), default), Times.Never);
    }

    [Fact]
    public async Task ProcessDataAsync_InvalidFormat_ReturnsNull()
    {
        // Arrange
        var data = "invalid data";
        var temperature = 0f;
        var humidity = 0f;

        _parserMock.Setup(p => p.TryParse(data, out temperature, out humidity))
            .Returns(false);

        // Act
        var result = await _service.ProcessDataAsync(data);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SensorReading>(), default), Times.Never);
    }

    [Fact]
    public async Task ProcessDataAsync_RepositoryThrowsException_ReturnsNull()
    {
        // Arrange
        var data = "temp=25.50,hum=60.00";
        var temperature = 25.50f;
        var humidity = 60.00f;

        _parserMock.Setup(p => p.TryParse(data, out temperature, out humidity))
            .Returns(true);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SensorReading>(), default))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _service.ProcessDataAsync(data);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SensorDataService(null!, _parserMock.Object, _loggerMock.Object));
        exception.ParamName.Should().Be("repository");
    }

    [Fact]
    public void Constructor_NullParser_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SensorDataService(_repositoryMock.Object, null!, _loggerMock.Object));
        exception.ParamName.Should().Be("parser");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SensorDataService(_repositoryMock.Object, _parserMock.Object, null!));
        exception.ParamName.Should().Be("logger");
    }
}
