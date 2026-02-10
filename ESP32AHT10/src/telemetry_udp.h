#pragma once

#include "telemetry.h"

#include "sdkconfig.h"

#if CONFIG_AHT10_UDP_ENABLE
#include "udp_sender.h"
#include "wifi_manager.h"

// UDP telemetry publisher context.
typedef struct {
    UdpSender *sender;
#if CONFIG_AHT10_WIFI_ENABLE
    const WifiManager *wifi;
#endif
} UdpPublisherCtx;

void telemetry_udp_publish(void *ctx, float temperature_c, float humidity);
#endif
