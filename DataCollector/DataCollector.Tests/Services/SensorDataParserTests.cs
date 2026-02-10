using DataCollector.Core.Services;
using FluentAssertions;
using Xunit;

namespace DataCollector.Tests.Services;

public class SensorDataParserTests
{
    private readonly SensorDataParser _parser;

    public SensorDataParserTests()
    {
        _parser = new SensorDataParser();
    }

    [Fact]
    public void TryParse_ValidData_ReturnsTrue()
    {
        // Arrange
        var data = "temp=25.50,hum=60.00";

        // Act
        var result = _parser.TryParse(data, out var temperature, out var humidity);

        // Assert
        result.Should().BeTrue();
        temperature.Should().Be(25.50f);
        humidity.Should().Be(60.00f);
    }

    [Theory]
    [InlineData("temp=20.00,hum=50.00", 20.00f, 50.00f)]
    [InlineData("temp=0.00,hum=0.00", 0.00f, 0.00f)]
    [InlineData("temp=-10.50,hum=100.00", -10.50f, 100.00f)]
    [InlineData("temp=35.75,hum=85.25", 35.75f, 85.25f)]
    public void TryParse_ValidDataVariations_ReturnsCorrectValues(string data, float expectedTemp, float expectedHum)
    {
        // Act
        var result = _parser.TryParse(data, out var temperature, out var humidity);

        // Assert
        result.Should().BeTrue();
        temperature.Should().BeApproximately(expectedTemp, 0.01f);
        humidity.Should().BeApproximately(expectedHum, 0.01f);
    }

    [Theory]
    [InlineData("TEMP=25.50,HUM=60.00")] // Case insensitive
    [InlineData("Temp=25.50,Hum=60.00")]
    public void TryParse_CaseInsensitive_ReturnsTrue(string data)
    {
        // Act
        var result = _parser.TryParse(data, out var temperature, out var humidity);

        // Assert
        result.Should().BeTrue();
        temperature.Should().Be(25.50f);
        humidity.Should().Be(60.00f);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryParse_EmptyOrNullData_ReturnsFalse(string? data)
    {
        // Act
        var result = _parser.TryParse(data!, out var temperature, out var humidity);

        // Assert
        result.Should().BeFalse();
        temperature.Should().Be(0f);
        humidity.Should().Be(0f);
    }

    [Theory]
    [InlineData("invalid data")]
    [InlineData("temp=25.50")]
    [InlineData("hum=60.00")]
    [InlineData("temperature=25.50,humidity=60.00")]
    [InlineData("temp=abc,hum=60.00")]
    [InlineData("temp=25.50,hum=xyz")]
    public void TryParse_InvalidFormat_ReturnsFalse(string data)
    {
        // Act
        var result = _parser.TryParse(data, out var temperature, out var humidity);

        // Assert
        result.Should().BeFalse();
    }
}
