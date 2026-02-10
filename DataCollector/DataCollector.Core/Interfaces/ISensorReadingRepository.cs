using DataCollector.Core.Models;

namespace DataCollector.Core.Interfaces;

/// <summary>
/// Repository interface for sensor readings following Repository Pattern (SOLID - Dependency Inversion Principle)
/// </summary>
public interface ISensorReadingRepository
{
    /// <summary>
    /// Adds a new sensor reading to the repository
    /// </summary>
    /// <param name="reading">The sensor reading to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added reading with generated ID</returns>
    Task<SensorReading> AddAsync(SensorReading reading, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets readings within a time range
    /// </summary>
    /// <param name="startTime">Start of the time range</param>
    /// <param name="endTime">End of the time range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of readings in the time range</returns>
    Task<IEnumerable<SensorReading>> GetReadingsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent reading
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most recent reading or null if none exist</returns>
    Task<SensorReading?> GetLatestReadingAsync(CancellationToken cancellationToken = default);
}
