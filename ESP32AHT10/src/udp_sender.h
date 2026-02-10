#pragma once

#include <stdint.h>

#include "sdkconfig.h"

#if CONFIG_AHT10_UDP_ENABLE
#include "lwip/sockets.h"
#include "lwip/inet.h"

// UDP sender.
typedef struct {
    int socket_fd;
    struct sockaddr dest_addr;
    socklen_t dest_len;
} UdpSender;

void udp_sender_init(UdpSender *sender, const char *host, uint16_t port, const char *log_tag);
void udp_sender_send(UdpSender *sender, const char *payload);
#endif
