# Quick Start Guide - ESP32 AHT10 Data Collector

This guide will help you get the temperature and humidity data collection system running on your Raspberry Pi.

## Prerequisites

1. **Raspberry Pi** with Raspberry Pi OS installed
2. **Docker** and **Docker Compose** installed on Raspberry Pi
3. **ESP32 with AHT10 sensor** (see firmware in `../ESP32AHT10`)
4. Both devices on the same network

## Step-by-Step Installation

### Step 1: Install Docker on Raspberry Pi

If Docker is not already installed:

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

Log out and log back in for the group change to take effect.

### Step 2: Install Docker Compose

```bash
sudo apt-get update
sudo apt-get install docker-compose
```

### Step 3: Clone or Copy the Repository

```bash
git clone https://github.com/bigboss2944/ESP32AHT10.git
cd ESP32AHT10/DataCollector
```

Or copy the DataCollector directory to your Raspberry Pi.

### Step 4: Start the Services

```bash
docker-compose up -d
```

This command will:
- Build the C#/.NET data collector service
- Start the data collector (listening on UDP port 5000)
- Start Grafana (accessible on port 3000)

### Step 5: Verify Services are Running

```bash
docker-compose ps
```

You should see two containers running:
- `sensor-datacollector`
- `sensor-grafana`

### Step 6: Configure ESP32

1. Find your Raspberry Pi's IP address:
   ```bash
   hostname -I
   ```

2. Update the ESP32 firmware configuration:
   - Set `CONFIG_AHT10_UDP_HOST` to your Raspberry Pi's IP
   - Set `CONFIG_AHT10_UDP_PORT` to `5000`
   - Rebuild and flash the ESP32 firmware

### Step 7: Access Grafana

1. Open a web browser and navigate to: `http://[RASPBERRY_PI_IP]:3000`
2. Login with:
   - Username: `admin`
   - Password: `admin`
3. You'll be prompted to change the password (optional)
4. The "Sensor Data Dashboard" should be automatically available

### Step 8: Verify Data Collection

Check the logs to ensure data is being received:

```bash
docker logs sensor-datacollector
```

You should see messages like:
```
info: DataCollector.Api.Services.UdpListenerService[0]
      UDP Listener started on port 5000
info: DataCollector.Core.Services.SensorDataService[0]
      Stored reading: Temp=25.5°C, Humidity=60.0%, Device=192.168.1.100
```

## Testing Without ESP32

You can test the system by sending test data manually:

### From Linux/Mac:
```bash
echo "temp=25.50,hum=60.00" | nc -u [RASPBERRY_PI_IP] 5000
```

### From Windows PowerShell:
```powershell
$udpClient = New-Object System.Net.Sockets.UdpClient
$endpoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Parse("[RASPBERRY_PI_IP]"), 5000)
$bytes = [System.Text.Encoding]::ASCII.GetBytes("temp=25.50,hum=60.00")
$udpClient.Send($bytes, $bytes.Length, $endpoint)
$udpClient.Close()
```

Replace `[RASPBERRY_PI_IP]` with your Raspberry Pi's actual IP address.

## Monitoring and Maintenance

### View Logs

Data Collector logs:
```bash
docker logs -f sensor-datacollector
```

Grafana logs:
```bash
docker logs -f sensor-grafana
```

### Stop Services

```bash
docker-compose down
```

### Restart Services

```bash
docker-compose restart
```

### Backup Database

```bash
docker cp sensor-datacollector:/data/sensordata.db ~/sensor-backup-$(date +%Y%m%d).db
```

### Update Services

```bash
docker-compose pull
docker-compose up -d
```

## Troubleshooting

### Problem: No data appearing in Grafana

**Check 1**: Verify data is being received
```bash
docker logs sensor-datacollector | grep "Stored reading"
```

**Check 2**: Verify database exists
```bash
docker exec sensor-datacollector ls -la /data/
```

**Check 3**: Test UDP connectivity
```bash
# On Raspberry Pi
sudo ufw allow 5000/udp
netstat -uln | grep 5000
```

### Problem: Cannot access Grafana

**Check 1**: Verify Grafana is running
```bash
docker ps | grep grafana
```

**Check 2**: Check Grafana logs
```bash
docker logs sensor-grafana
```

**Check 3**: Verify firewall
```bash
sudo ufw allow 3000/tcp
```

### Problem: ESP32 cannot connect

**Check 1**: Verify ESP32 and Raspberry Pi are on same network

**Check 2**: Test UDP from ESP32's network
```bash
echo "temp=25.50,hum=60.00" | nc -u [RASPBERRY_PI_IP] 5000
```

**Check 3**: Check router/firewall settings

## Performance Tuning

### For High-Frequency Data

If receiving data more frequently than every 2 seconds, consider:

1. Adjust database indexes (already optimized)
2. Increase Docker container resources:
   ```yaml
   # In docker-compose.yml
   datacollector:
     deploy:
       resources:
         limits:
           cpus: '0.5'
           memory: 512M
   ```

### For Limited Storage

Configure database cleanup job:
```sql
-- Delete readings older than 30 days
DELETE FROM SensorReadings WHERE Timestamp < datetime('now', '-30 days');
```

You can set this up as a cron job on the Raspberry Pi.

## Advanced Configuration

### Custom Port

To use a different UDP port, edit `docker-compose.yml`:

```yaml
datacollector:
  ports:
    - "6000:6000/udp"  # Changed from 5000
  environment:
    - UdpListener__Port=6000  # Must match
```

### Multiple Sensors

The system automatically tracks device IDs (IP addresses). All sensors sending to the same port will be stored in the same database with their respective DeviceId.

### External Database Access

To query the database from outside Docker:

```bash
# Export database
docker cp sensor-datacollector:/data/sensordata.db ./sensordata.db

# Query with sqlite3
sqlite3 sensordata.db "SELECT * FROM SensorReadings ORDER BY Timestamp DESC LIMIT 10"
```

## Support

For issues, questions, or contributions:
- Repository: https://github.com/bigboss2944/ESP32AHT10
- Create an issue with detailed description
- Include logs when reporting problems

## Next Steps

- Customize the Grafana dashboard
- Add alerts for temperature/humidity thresholds
- Export data for analysis
- Set up automated backups
- Configure SSL/TLS for Grafana (for remote access)

Enjoy monitoring your sensor data! 🌡️💧
