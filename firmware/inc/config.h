#ifndef CONFIG_H
#define CONFIG_H

#define LED_PIN (24)
#define HARP_CORE_LED_PIN (25)

// #define DEBUG /////////
// #define BME680_DEBUG

#define BME688_CS_PIN (20)
#define BME688_POCI_PIN (16)
#define BME688_PICO_PIN (19)
#define BME688_SCK_PIN (18)

#define HARP_SYNC_UART_ID (uart1)
#define HARP_SYNC_RX_PIN (5)

#define HW_VERSION_MAJOR (1)
#define HW_VERSION_MINOR (0)
#define HW_VERSION_PATCH (0)

#define DEVICE_ASSEMBLY_VERSION (2)

#define FW_VERSION_MAJOR (0)
#define FW_VERSION_MINOR (1)
#define FW_VERSION_PATCH (0)
#define DEVICE_SERIAL_NUMBER (0)

#define HARP_DEVICE_NAME "EnvironmentSensor"

#define ENV_SENSOR_DEVICE_ID (0x057D) // 1405

#endif // CONFIG_H
