#pragma once

#include <stdint.h>
#include <stddef.h>

#include "driver/i2c.h"
#include "esp_err.h"

// Simple I2C bus configuration and operations.
typedef struct {
    i2c_port_t port;
    gpio_num_t sda;
    gpio_num_t scl;
    uint32_t frequency_hz;
    uint32_t timeout_ms;
} I2cBus;

esp_err_t i2c_bus_init(const I2cBus *bus);
void i2c_bus_scan(const I2cBus *bus, const char *log_tag);

esp_err_t i2c_bus_write(const I2cBus *bus, uint8_t addr, const uint8_t *data, size_t len);
esp_err_t i2c_bus_read(const I2cBus *bus, uint8_t addr, uint8_t *data, size_t len);
