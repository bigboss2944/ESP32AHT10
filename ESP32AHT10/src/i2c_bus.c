#include "i2c_bus.h"

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_log.h"

// Initialize the I2C master bus.
esp_err_t i2c_bus_init(const I2cBus *bus)
{
    i2c_config_t conf = {
        .mode = I2C_MODE_MASTER,
        .sda_io_num = bus->sda,
        .scl_io_num = bus->scl,
        .sda_pullup_en = GPIO_PULLUP_ENABLE,
        .scl_pullup_en = GPIO_PULLUP_ENABLE,
        .master.clk_speed = bus->frequency_hz,
    };
    ESP_ERROR_CHECK(i2c_param_config(bus->port, &conf));
    return i2c_driver_install(bus->port, conf.mode, 0, 0, 0);
}

// Scan the I2C bus and log any discovered devices.
void i2c_bus_scan(const I2cBus *bus, const char *log_tag)
{
    ESP_LOGI(log_tag, "I2C scan on SDA=%d SCL=%d", bus->sda, bus->scl);
    for (uint8_t addr = 0x08; addr < 0x78; addr++) {
        i2c_cmd_handle_t cmd = i2c_cmd_link_create();
        i2c_master_start(cmd);
        i2c_master_write_byte(cmd, (addr << 1) | I2C_MASTER_WRITE, true);
        i2c_master_stop(cmd);
        esp_err_t ret = i2c_master_cmd_begin(bus->port, cmd, pdMS_TO_TICKS(20));
        i2c_cmd_link_delete(cmd);
        if (ret == ESP_OK) {
            ESP_LOGI(log_tag, "I2C device found at 0x%02X", addr);
        }
    }
}

// Write a raw buffer to a device on the I2C bus.
esp_err_t i2c_bus_write(const I2cBus *bus, uint8_t addr, const uint8_t *data, size_t len)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (addr << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write(cmd, (uint8_t *)data, len, true);
    i2c_master_stop(cmd);
    esp_err_t ret = i2c_master_cmd_begin(bus->port, cmd, pdMS_TO_TICKS(bus->timeout_ms));
    i2c_cmd_link_delete(cmd);
    return ret;
}

// Read a raw buffer from a device on the I2C bus.
esp_err_t i2c_bus_read(const I2cBus *bus, uint8_t addr, uint8_t *data, size_t len)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (addr << 1) | I2C_MASTER_READ, true);
    if (len > 1) {
        i2c_master_read(cmd, data, len - 1, I2C_MASTER_ACK);
    }
    i2c_master_read_byte(cmd, data + len - 1, I2C_MASTER_NACK);
    i2c_master_stop(cmd);
    esp_err_t ret = i2c_master_cmd_begin(bus->port, cmd, pdMS_TO_TICKS(bus->timeout_ms));
    i2c_cmd_link_delete(cmd);
    return ret;
}
