cmake_minimum_required(VERSION 3.16.0)

# Le composant principal est dans src/ (au lieu de main/)
set(EXTRA_COMPONENT_DIRS "${CMAKE_SOURCE_DIR}/src")

include($ENV{IDF_PATH}/tools/cmake/project.cmake)
project(ESP32AHT10)
