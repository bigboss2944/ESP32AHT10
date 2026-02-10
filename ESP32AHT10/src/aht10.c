#include "aht10.h"

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#define AHT10_CMD_RESET 0xBA
#define AHT10_CMD_INIT  0xE1
#define AHT10_CMD_MEASURE 0xAC

// Initialize the AHT10 sensor (soft reset + init).
esp_err_t aht10_init(const Aht10Sensor *sensor)
{
    const uint8_t reset_cmd[] = {AHT10_CMD_RESET};
    const uint8_t init_cmd[] = {AHT10_CMD_INIT, 0x08, 0x00};

    esp_err_t ret = i2c_bus_write(sensor->bus, sensor->address, reset_cmd, sizeof(reset_cmd));
    if (ret != ESP_OK) {
        return ret;
    }
    vTaskDelay(pdMS_TO_TICKS(20));
    return i2c_bus_write(sensor->bus, sensor->address, init_cmd, sizeof(init_cmd));
}

// Read temperature and humidity from AHT10.
esp_err_t aht10_read(const Aht10Sensor *sensor, float *temperature_c, float *humidity)
{
    const uint8_t measure_cmd[] = {AHT10_CMD_MEASURE, 0x33, 0x00};
    uint8_t data[6] = {0};

    esp_err_t ret = i2c_bus_write(sensor->bus, sensor->address, measure_cmd, sizeof(measure_cmd));
    if (ret != ESP_OK) {
        return ret;
    }

    vTaskDelay(pdMS_TO_TICKS(80));
    ret = i2c_bus_read(sensor->bus, sensor->address, data, sizeof(data));
    if (ret != ESP_OK) {
        return ret;
    }

    if (data[0] & 0x80) {
        return ESP_ERR_TIMEOUT;
    }

    uint32_t raw_humidity = ((uint32_t)data[1] << 12) | ((uint32_t)data[2] << 4) | (data[3] >> 4);
    uint32_t raw_temp = ((uint32_t)(data[3] & 0x0F) << 16) | ((uint32_t)data[4] << 8) | data[5];

    *humidity = ((float)raw_humidity / 1048576.0f) * 100.0f;
    *temperature_c = ((float)raw_temp / 1048576.0f) * 200.0f - 50.0f;

    return ESP_OK;
}
