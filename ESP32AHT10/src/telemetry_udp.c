#include "telemetry_udp.h"

#if CONFIG_AHT10_UDP_ENABLE

#include <stdio.h>

// UDP publisher for telemetry.
void telemetry_udp_publish(void *ctx, float temperature_c, float humidity)
{
    UdpPublisherCtx *publisher_ctx = (UdpPublisherCtx *)ctx;
#if CONFIG_AHT10_WIFI_ENABLE
    if (!wifi_manager_is_connected(publisher_ctx->wifi)) {
        return;
    }
#endif
    char msg[96];
    snprintf(msg, sizeof(msg), "temp=%.2f,hum=%.2f", temperature_c, humidity);
    udp_sender_send(publisher_ctx->sender, msg);
}

#endif
