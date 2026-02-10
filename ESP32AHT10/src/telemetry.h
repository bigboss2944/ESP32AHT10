#pragma once

// Publisher interface for telemetry.
typedef struct {
    void (*publish)(void *ctx, float temperature_c, float humidity);
    void *ctx;
} TelemetryPublisher;

void telemetry_noop_publish(void *ctx, float temperature_c, float humidity);
