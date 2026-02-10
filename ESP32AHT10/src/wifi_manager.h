#pragma once

#include <stdbool.h>

#include "esp_err.h"
#include "freertos/FreeRTOS.h"
#include "freertos/event_groups.h"

#include "sdkconfig.h"

#if CONFIG_AHT10_WIFI_ENABLE
// WiFi manager for STA mode.
typedef struct {
    EventGroupHandle_t event_group;
} WifiManager;

esp_err_t wifi_manager_init(WifiManager *manager, const char *ssid, const char *password, const char *log_tag);
bool wifi_manager_is_connected(const WifiManager *manager);
#endif
