#include <stdio.h>
#include <string.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#include "esp_log.h"
#include "esp_err.h"
#include "nvs_flash.h"
#include "sdkconfig.h"

#include "i2c_bus.h"
#include "aht10.h"
#include "wifi_manager.h"
#include "udp_sender.h"
#include "telemetry.h"
#include "telemetry_udp.h"

static const char *TAG = "AHT10";

// Fallbacks for editor tooling when sdkconfig.h isn't fully indexed.
#ifndef CONFIG_AHT10_I2C_SDA
#define CONFIG_AHT10_I2C_SDA 11
#endif
#ifndef CONFIG_AHT10_I2C_SCL
#define CONFIG_AHT10_I2C_SCL 12
#endif
#ifndef CONFIG_AHT10_SAMPLE_PERIOD_MS
#define CONFIG_AHT10_SAMPLE_PERIOD_MS 2000
#endif

#define AHT10_I2C_ADDR 0x38

#define I2C_MASTER_NUM           I2C_NUM_0
#define I2C_MASTER_SCL_IO        CONFIG_AHT10_I2C_SCL
#define I2C_MASTER_SDA_IO        CONFIG_AHT10_I2C_SDA
#define I2C_MASTER_FREQ_HZ       100000
#define I2C_MASTER_TIMEOUT_MS    1000

// Context for the sensor task loop.
typedef struct {
    Aht10Sensor sensor;
    TelemetryPublisher publisher;
} SensorTaskCtx;

// Main sensor loop task.
static void sensor_task(void *arg)
{
    const SensorTaskCtx *ctx = (const SensorTaskCtx *)arg;
    while (1) {
        float temperature = 0.0f;
        float humidity = 0.0f;

        esp_err_t ret = aht10_read(&ctx->sensor, &temperature, &humidity);
        if (ret == ESP_OK) {
            ESP_LOGI(TAG, "Temp: %.2f C | Humidity: %.2f %%", temperature, humidity);
            ctx->publisher.publish(ctx->publisher.ctx, temperature, humidity);
        } else {
            ESP_LOGW(TAG, "AHT10 read failed: %s", esp_err_to_name(ret));
        }

        vTaskDelay(pdMS_TO_TICKS(CONFIG_AHT10_SAMPLE_PERIOD_MS));
    }
}

void app_main(void)
{
    ESP_LOGI(TAG, "Boot OK");

    // Hardware setup.
    static const I2cBus i2c_bus = {
        .port = I2C_MASTER_NUM,
        .sda = I2C_MASTER_SDA_IO,
        .scl = I2C_MASTER_SCL_IO,
        .frequency_hz = I2C_MASTER_FREQ_HZ,
        .timeout_ms = I2C_MASTER_TIMEOUT_MS,
    };

    static const Aht10Sensor aht10 = {
        .bus = &i2c_bus,
        .address = AHT10_I2C_ADDR,
    };

    ESP_ERROR_CHECK(i2c_bus_init(&i2c_bus));
    vTaskDelay(pdMS_TO_TICKS(40));
    i2c_bus_scan(&i2c_bus, TAG);

    esp_err_t ret = aht10_init(&aht10);
    if (ret != ESP_OK) {
        ESP_LOGE(TAG, "AHT10 init failed: %s", esp_err_to_name(ret));
    }

    // Communication setup.
    TelemetryPublisher publisher = {
        .publish = telemetry_noop_publish,
        .ctx = NULL,
    };

#if CONFIG_AHT10_WIFI_ENABLE
    static WifiManager wifi_manager = {0};
    ESP_ERROR_CHECK(nvs_flash_init());
    ESP_ERROR_CHECK(wifi_manager_init(&wifi_manager, CONFIG_AHT10_WIFI_SSID, CONFIG_AHT10_WIFI_PASSWORD, TAG));
#if CONFIG_AHT10_UDP_ENABLE
    static UdpSender udp_sender = {
        .socket_fd = -1,
    };
    static UdpPublisherCtx udp_ctx = {
        .sender = &udp_sender,
        .wifi = &wifi_manager,
    };
    udp_sender_init(&udp_sender, CONFIG_AHT10_UDP_HOST, CONFIG_AHT10_UDP_PORT, TAG);
    publisher.publish = telemetry_udp_publish;
    publisher.ctx = &udp_ctx;
#endif
#endif

    static SensorTaskCtx task_ctx = {
        .sensor = {
            .bus = &i2c_bus,
            .address = AHT10_I2C_ADDR,
        },
        .publisher = {
            .publish = telemetry_noop_publish,
            .ctx = NULL,
        },
    };

    // Copy publisher config into the task context.
    task_ctx.publisher = publisher;

    xTaskCreate(sensor_task, "sensor_task", 4096, (void *)&task_ctx, 5, NULL);
}
