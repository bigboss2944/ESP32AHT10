using DataCollector.Core.Interfaces;
using DataCollector.Core.Models;
using Microsoft.Extensions.Logging;

namespace DataCollector.Core.Services;

/// <summary>
/// Service for processing sensor data (SOLID - Single Responsibility Principle)
/// </summary>
public class SensorDataService : ISensorDataService
{
    private readonly ISensorReadingRepository _repository;
    private readonly IDataParser _parser;
    private readonly ILogger<SensorDataService> _logger;

    public SensorDataService(
        ISensorReadingRepository repository,
        IDataParser parser,
        ILogger<SensorDataService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SensorReading?> ProcessDataAsync(string data, string? deviceId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            _logger.LogWarning("Received empty or null data");
            return null;
        }

        if (!_parser.TryParse(data, out var temperature, out var humidity))
        {
            _logger.LogWarning("Failed to parse data: {Data}", data);
            return null;
        }

        var reading = new SensorReading
        {
            Temperature = temperature,
            Humidity = humidity,
            Timestamp = DateTime.UtcNow,
            DeviceId = deviceId
        };

        try
        {
            var savedReading = await _repository.AddAsync(reading, cancellationToken);
            _logger.LogInformation(
                "Stored reading: Temp={Temperature}°C, Humidity={Humidity}%, Device={DeviceId}",
                temperature, humidity, deviceId ?? "unknown");
            return savedReading;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store sensor reading");
            return null;
        }
    }
}
