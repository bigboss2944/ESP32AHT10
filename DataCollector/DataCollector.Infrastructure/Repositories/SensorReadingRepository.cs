using DataCollector.Core.Interfaces;
using DataCollector.Core.Models;
using DataCollector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataCollector.Infrastructure.Repositories;

/// <summary>
/// SQLite repository implementation for sensor readings (SOLID - Dependency Inversion Principle)
/// </summary>
public class SensorReadingRepository : ISensorReadingRepository
{
    private readonly SensorDataContext _context;

    public SensorReadingRepository(SensorDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SensorReading> AddAsync(SensorReading reading, CancellationToken cancellationToken = default)
    {
        if (reading == null)
        {
            throw new ArgumentNullException(nameof(reading));
        }

        _context.SensorReadings.Add(reading);
        await _context.SaveChangesAsync(cancellationToken);
        return reading;
    }

    public async Task<IEnumerable<SensorReading>> GetReadingsAsync(
        DateTime startTime, 
        DateTime endTime, 
        CancellationToken cancellationToken = default)
    {
        return await _context.SensorReadings
            .Where(r => r.Timestamp >= startTime && r.Timestamp <= endTime)
            .OrderBy(r => r.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<SensorReading?> GetLatestReadingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SensorReadings
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
