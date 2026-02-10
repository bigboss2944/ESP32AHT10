using DataCollector.Core.Models;

namespace DataCollector.Core.Interfaces;

/// <summary>
/// Service interface for processing sensor data (SOLID - Single Responsibility Principle)
/// </summary>
public interface ISensorDataService
{
    /// <summary>
    /// Processes incoming sensor data
    /// </summary>
    /// <param name="data">Raw sensor data string</param>
    /// <param name="deviceId">Optional device identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The stored sensor reading or null if parsing failed</returns>
    Task<SensorReading?> ProcessDataAsync(string data, string? deviceId = null, CancellationToken cancellationToken = default);
}
