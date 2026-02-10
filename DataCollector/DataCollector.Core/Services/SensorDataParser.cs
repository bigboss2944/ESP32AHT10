using DataCollector.Core.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DataCollector.Core.Services;

/// <summary>
/// Parser for sensor data in the format: "temp=25.50,hum=60.00" (SOLID - Single Responsibility Principle)
/// </summary>
public class SensorDataParser : IDataParser
{
    private static readonly Regex DataRegex = new(@"temp=(-?[\d.]+),hum=(-?[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool TryParse(string data, out float temperature, out float humidity)
    {
        temperature = 0f;
        humidity = 0f;

        if (string.IsNullOrWhiteSpace(data))
        {
            return false;
        }

        var match = DataRegex.Match(data);
        if (!match.Success)
        {
            return false;
        }

        return float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out temperature) &&
               float.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out humidity);
    }
}
