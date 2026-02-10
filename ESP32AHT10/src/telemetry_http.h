#pragma once

#include <stdint.h>

#include "esp_err.h"
#include "esp_http_server.h"

#include "telemetry_store.h"

typedef struct {
    httpd_handle_t server;
    const TelemetryStore *store;
    const char *log_tag;
} TelemetryHttpServer;

esp_err_t telemetry_http_server_start(TelemetryHttpServer *server,
                                      const TelemetryStore *store,
                                      const char *log_tag,
                                      uint16_t port,
                                      const char *uri_path);
void telemetry_http_server_stop(TelemetryHttpServer *server);
