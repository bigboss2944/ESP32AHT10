#include "telemetry_http.h"

#if CONFIG_AHT10_HTTP_ENABLE

#include <inttypes.h>
#include <stdio.h>

#include "esp_log.h"

static esp_err_t telemetry_get_handler(httpd_req_t *req)
{
    TelemetryHttpServer *server = (TelemetryHttpServer *)req->user_ctx;
    TelemetrySample sample = {0};
    bool has_data = telemetry_store_get(server->store, &sample);

    httpd_resp_set_type(req, "application/json");

    if (!has_data) {
        httpd_resp_set_status(req, "503 Service Unavailable");
        return httpd_resp_send(req, "{\"error\":\"no_data\"}", HTTPD_RESP_USE_STRLEN);
    }

    char payload[160];
    snprintf(payload,
             sizeof(payload),
             "{\"temperature_c\":%.2f,\"humidity\":%.2f,\"timestamp_ms\":%" PRId64 "}",
             sample.temperature_c,
             sample.humidity,
             sample.timestamp_ms);

    return httpd_resp_send(req, payload, HTTPD_RESP_USE_STRLEN);
}

esp_err_t telemetry_http_server_start(TelemetryHttpServer *server,
                                      const TelemetryStore *store,
                                      const char *log_tag,
                                      uint16_t port,
                                      const char *uri_path)
{
    if (!server || !store || !uri_path) {
        return ESP_ERR_INVALID_ARG;
    }

    server->store = store;
    server->log_tag = log_tag;

    httpd_config_t config = HTTPD_DEFAULT_CONFIG();
    config.server_port = port;

    esp_err_t err = httpd_start(&server->server, &config);
    if (err != ESP_OK) {
        ESP_LOGE(log_tag, "HTTP server start failed: %s", esp_err_to_name(err));
        return err;
    }

    httpd_uri_t telemetry_uri = {
        .uri = uri_path,
        .method = HTTP_GET,
        .handler = telemetry_get_handler,
        .user_ctx = server,
    };

    err = httpd_register_uri_handler(server->server, &telemetry_uri);
    if (err != ESP_OK) {
        ESP_LOGE(log_tag, "Failed to register HTTP handler: %s", esp_err_to_name(err));
        httpd_stop(server->server);
        server->server = NULL;
        return err;
    }

    ESP_LOGI(log_tag, "HTTP telemetry endpoint ready on %s", uri_path);
    return ESP_OK;
}

void telemetry_http_server_stop(TelemetryHttpServer *server)
{
    if (server && server->server) {
        httpd_stop(server->server);
        server->server = NULL;
    }
}

#endif
