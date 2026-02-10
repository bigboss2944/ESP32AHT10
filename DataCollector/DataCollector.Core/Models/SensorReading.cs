namespace DataCollector.Core.Models;

/// <summary>
/// Represents a temperature and humidity sensor reading
/// </summary>
public class SensorReading
{
    /// <summary>
    /// Unique identifier for the reading
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    public float Temperature { get; set; }

    /// <summary>
    /// Humidity percentage (0-100)
    /// </summary>
    public float Humidity { get; set; }

    /// <summary>
    /// Timestamp when the reading was recorded
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Optional device identifier
    /// </summary>
    public string? DeviceId { get; set; }
}
