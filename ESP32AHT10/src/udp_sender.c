#include "udp_sender.h"

#if CONFIG_AHT10_UDP_ENABLE

#include <errno.h>
#include <string.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>

#include "esp_log.h"

// Initialize a UDP sender.
void udp_sender_init(UdpSender *sender, const char *host, uint16_t port, const char *log_tag)
{
    sender->socket_fd = socket(AF_INET, SOCK_DGRAM, IPPROTO_IP);
    if (sender->socket_fd < 0) {
        ESP_LOGE(log_tag, "Unable to create UDP socket: errno %d", errno);
        return;
    }

    struct sockaddr_in dest_in = {
        .sin_family = AF_INET,
        .sin_port = htons(port),
    };
    if (inet_pton(AF_INET, host, &dest_in.sin_addr) != 1) {
        ESP_LOGE(log_tag, "Invalid UDP host: %s", host);
        close(sender->socket_fd);
        sender->socket_fd = -1;
    }
    memcpy(&sender->dest_addr, &dest_in, sizeof(dest_in));
    sender->dest_len = sizeof(dest_in);
}

// Send a UDP packet.
void udp_sender_send(UdpSender *sender, const char *payload)
{
    if (sender->socket_fd < 0) {
        return;
    }
    sendto(sender->socket_fd, payload, strlen(payload), 0,
           (struct sockaddr *)&sender->dest_addr, sender->dest_len);
}

#endif
