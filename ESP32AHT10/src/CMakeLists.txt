# This file was automatically generated for projects
# without default 'CMakeLists.txt' file.
idf_component_register(
	SRCS "main.c"
	"i2c_bus.c"
	"aht10.c"
	"wifi_manager.c"
	"udp_sender.c"
	"telemetry.c"
	"telemetry_udp.c"
	INCLUDE_DIRS "."
	REQUIRES nvs_flash esp_wifi esp_netif esp_event driver lwip
)