#include "telemetry.h"

// No-op publisher when telemetry is disabled.
void telemetry_noop_publish(void *ctx, float temperature_c, float humidity)
{
    (void)ctx;
    (void)temperature_c;
    (void)humidity;
}
