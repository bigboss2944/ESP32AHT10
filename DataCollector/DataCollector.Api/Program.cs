using DataCollector.Api.Services;
using DataCollector.Core.Interfaces;
using DataCollector.Core.Services;
using DataCollector.Infrastructure.Data;
using DataCollector.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Configure SQLite database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=sensordata.db";

builder.Services.AddDbContext<SensorDataContext>(options =>
    options.UseSqlite(connectionString));

// Register services following Dependency Injection (SOLID - Dependency Inversion Principle)
builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
builder.Services.AddSingleton<IDataParser, SensorDataParser>();
builder.Services.AddScoped<ISensorDataService, SensorDataService>();

// Register UDP listener service
builder.Services.AddHostedService<UdpListenerService>();

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SensorDataContext>();
    context.Database.EnsureCreated();
}

host.Run();
