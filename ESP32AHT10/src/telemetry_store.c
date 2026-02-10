#include "telemetry_store.h"

#include "esp_timer.h"

void telemetry_store_init(TelemetryStore *store)
{
    if (!store) {
        return;
    }
    store->mutex = xSemaphoreCreateMutex();
    store->sample.temperature_c = 0.0f;
    store->sample.humidity = 0.0f;
    store->sample.timestamp_ms = 0;
    store->sample.has_data = false;
}

void telemetry_store_update(TelemetryStore *store, float temperature_c, float humidity)
{
    if (!store || !store->mutex) {
        return;
    }
    if (xSemaphoreTake(store->mutex, portMAX_DELAY) == pdTRUE) {
        store->sample.temperature_c = temperature_c;
        store->sample.humidity = humidity;
        store->sample.timestamp_ms = esp_timer_get_time() / 1000;
        store->sample.has_data = true;
        xSemaphoreGive(store->mutex);
    }
}

bool telemetry_store_get(const TelemetryStore *store, TelemetrySample *out_sample)
{
    if (!store || !store->mutex || !out_sample) {
        return false;
    }
    bool has_data = false;
    if (xSemaphoreTake(store->mutex, portMAX_DELAY) == pdTRUE) {
        *out_sample = store->sample;
        has_data = store->sample.has_data;
        xSemaphoreGive(store->mutex);
    }
    return has_data;
}
