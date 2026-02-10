#pragma once

#include <stdbool.h>
#include <stdint.h>

#include "freertos/FreeRTOS.h"
#include "freertos/semphr.h"

typedef struct {
    float temperature_c;
    float humidity;
    int64_t timestamp_ms;
    bool has_data;
} TelemetrySample;

typedef struct {
    TelemetrySample sample;
    SemaphoreHandle_t mutex;
} TelemetryStore;

void telemetry_store_init(TelemetryStore *store);
void telemetry_store_update(TelemetryStore *store, float temperature_c, float humidity);
bool telemetry_store_get(const TelemetryStore *store, TelemetrySample *out_sample);
