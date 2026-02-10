#pragma once

#include <stdint.h>

#include "esp_err.h"
#include "i2c_bus.h"

// AHT10 sensor descriptor.
typedef struct {
    const I2cBus *bus;
    uint8_t address;
} Aht10Sensor;

esp_err_t aht10_init(const Aht10Sensor *sensor);
esp_err_t aht10_read(const Aht10Sensor *sensor, float *temperature_c, float *humidity);
