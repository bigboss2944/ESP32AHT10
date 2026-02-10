namespace DataCollector.Core.Interfaces;

/// <summary>
/// Interface for parsing sensor data (SOLID - Single Responsibility Principle)
/// </summary>
public interface IDataParser
{
    /// <summary>
    /// Parses temperature and humidity from raw data
    /// </summary>
    /// <param name="data">Raw data string</param>
    /// <param name="temperature">Parsed temperature value</param>
    /// <param name="humidity">Parsed humidity value</param>
    /// <returns>True if parsing was successful, false otherwise</returns>
    bool TryParse(string data, out float temperature, out float humidity);
}
