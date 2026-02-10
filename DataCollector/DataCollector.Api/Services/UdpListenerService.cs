using DataCollector.Core.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataCollector.Api.Services;

/// <summary>
/// Background service that listens for UDP sensor data (SOLID - Single Responsibility Principle)
/// </summary>
public class UdpListenerService : BackgroundService
{
    private readonly ISensorDataService _sensorDataService;
    private readonly ILogger<UdpListenerService> _logger;
    private readonly IConfiguration _configuration;
    private UdpClient? _udpClient;

    public UdpListenerService(
        ISensorDataService sensorDataService,
        ILogger<UdpListenerService> logger,
        IConfiguration configuration)
    {
        _sensorDataService = sensorDataService ?? throw new ArgumentNullException(nameof(sensorDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = _configuration.GetValue<int>("UdpListener:Port", 5000);
        
        try
        {
            _udpClient = new UdpClient(port);
            _logger.LogInformation("UDP Listener started on port {Port}", port);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(stoppingToken);
                    var data = Encoding.UTF8.GetString(result.Buffer);
                    var remoteEndPoint = result.RemoteEndPoint.Address.ToString();

                    _logger.LogDebug("Received data from {RemoteEndPoint}: {Data}", remoteEndPoint, data);

                    await _sensorDataService.ProcessDataAsync(data, remoteEndPoint, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving UDP data");
                    await Task.Delay(1000, stoppingToken); // Brief delay before retry
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start UDP listener on port {Port}", port);
            throw;
        }
        finally
        {
            _udpClient?.Dispose();
            _logger.LogInformation("UDP Listener stopped");
        }
    }

    public override void Dispose()
    {
        _udpClient?.Dispose();
        base.Dispose();
    }
}
