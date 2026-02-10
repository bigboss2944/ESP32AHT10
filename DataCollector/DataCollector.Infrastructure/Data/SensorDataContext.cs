using DataCollector.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCollector.Infrastructure.Data;

/// <summary>
/// Database context for sensor readings using Entity Framework Core
/// </summary>
public class SensorDataContext : DbContext
{
    public SensorDataContext(DbContextOptions<SensorDataContext> options)
        : base(options)
    {
    }

    public DbSet<SensorReading> SensorReadings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Temperature).IsRequired();
            entity.Property(e => e.Humidity).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.DeviceId).HasMaxLength(50);
            
            // Index on Timestamp for faster queries
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
