#include <pico/stdlib.h> // for uart printing
#include <cstring>
#include <stdio.h>
#include <config.h>
#include <pico/multicore.h>
#include <harp_c_app.h>
#include <harp_synchronizer.h>
#include <core_registers.h>
#include <reg_types.h>
#include "core1_sensor.h"

#include "hardware/pll.h"
#include "hardware/clocks.h"
#include "hardware/structs/pll.h"
#include "hardware/structs/clocks.h"

#define SYS_CLOCK_SPEED_MHZ (12)
#define TEMPERATURE_OFFSET_C (-5.49f)

static void configure_clock();

// Create device name array.
const uint16_t who_am_i = ENV_SENSOR_DEVICE_ID;
const uint8_t hw_version_major = HW_VERSION_MAJOR;
const uint8_t hw_version_minor = HW_VERSION_MINOR;
const uint8_t assembly_version = DEVICE_ASSEMBLY_VERSION;
const uint8_t harp_version_major = HARP_VERSION_MAJOR;
const uint8_t harp_version_minor = HARP_VERSION_MINOR;
const uint8_t fw_version_major = FW_VERSION_MAJOR;
const uint8_t fw_version_minor = FW_VERSION_MINOR;
const uint16_t serial_number = DEVICE_SERIAL_NUMBER;


void set_led_state(bool enabled)
{
    if (enabled)
    {
        gpio_init(LED_PIN);
        gpio_set_dir(LED_PIN, true); // true for output
        gpio_put(LED_PIN, 0);
    }
    else
        gpio_deinit(LED_PIN);
}


queue_t sensor_queue;
queue_t cmd_queue;

//////// Harp stuff

// Harp App Register Setup.
const size_t reg_count = 6;
 
// Define register contents.
#pragma pack(push, 1)
struct app_regs_t
{
    uint32_t pressure_pa;  // app register 0
    float temperature_c;         // app register 1
    float humidity_prh;         // app register 2
    float pressure_temp_humidity[3];
    uint8_t enable_sensor_dispatch_events;
    float temperature_offset_c;
} app_regs;
#pragma pack(pop)

// Define register "specs."
RegSpecs app_reg_specs[reg_count]
{
    {(uint8_t*)&app_regs.pressure_pa, sizeof(app_regs.pressure_pa), U32},
    {(uint8_t*)&app_regs.temperature_c, sizeof(app_regs.temperature_c), Float},
    {(uint8_t*)&app_regs.humidity_prh, sizeof(app_regs.humidity_prh), Float},
    {(uint8_t*)&app_regs.pressure_temp_humidity, sizeof(app_regs.pressure_temp_humidity), Float},
    {(uint8_t*)&app_regs.enable_sensor_dispatch_events, sizeof(app_regs.enable_sensor_dispatch_events), U8},
    {(uint8_t*)&app_regs.temperature_offset_c, sizeof(app_regs.temperature_offset_c), Float}
};

// Define register read-and-write handler functions.
RegFnPair reg_handler_fns[reg_count]
{
    {&HarpCore::read_reg_generic, &HarpCore::write_to_read_only_reg_error},
    {&HarpCore::read_reg_generic, &HarpCore::write_to_read_only_reg_error},
    {&HarpCore::read_reg_generic, &HarpCore::write_to_read_only_reg_error},
    {&HarpCore::read_reg_generic, &HarpCore::write_to_read_only_reg_error},
    {&HarpCore::read_reg_generic, &HarpCore::write_to_read_only_reg_error}
};

void app_reset()
{
    app_regs.pressure_pa = 0;
    app_regs.temperature_c = 0;
    app_regs.humidity_prh = 0;
    app_regs.pressure_temp_humidity[0] = 0;
    app_regs.pressure_temp_humidity[1] = 0;
    app_regs.pressure_temp_humidity[2] = 0;
    app_regs.temperature_offset_c = TEMPERATURE_OFFSET_C;

    app_regs.enable_sensor_dispatch_events = 1;

    uint8_t reset = 0xff;
    queue_add_blocking(&cmd_queue,&reset);
}

void update_app_state()
{
    // Get data from sensor-reading loop via queue and set registers
    if (!queue_is_empty(&sensor_queue)){
        sensor_data_t data;
        queue_remove_blocking(&sensor_queue, &data);

        data.temperature_c += TEMPERATURE_OFFSET_C;

        app_regs.pressure_pa = data.pressure_pa;
        app_regs.temperature_c = data.temperature_c;
        app_regs.humidity_prh = data.humidity_prh;
        app_regs.pressure_temp_humidity[0] = float(data.pressure_pa);
        app_regs.pressure_temp_humidity[1] = data.temperature_c;
        app_regs.pressure_temp_humidity[2] = data.humidity_prh;
        
        // Send an event for the aggregate register
        if (!HarpCore::is_muted() && HarpCore::events_enabled() && bool(app_regs.enable_sensor_dispatch_events))
            HarpCApp::send_harp_reply(EVENT, APP_REG_START_ADDRESS + 3);


    }

}

// Create Harp App.
HarpCApp& app = HarpCApp::init(who_am_i, hw_version_major, hw_version_minor,
                               assembly_version,
                               harp_version_major, harp_version_minor,
                               fw_version_major, fw_version_minor,
                               serial_number, HARP_DEVICE_NAME,
                               (const uint8_t*)GIT_HASH, // in CMakeLists.txt.
                               &app_regs, app_reg_specs,
                               reg_handler_fns, reg_count, update_app_state,
                               app_reset);



// Core0 main.
int main()
{
    // Init Synchronizer.
    HarpSynchronizer& sync = HarpSynchronizer::init(HARP_SYNC_UART_ID,
                                                    HARP_SYNC_RX_PIN);
    // app.set_visual_indicators_fn(set_led_state);
    app.set_synchronizer(&sync);

    // launch core1 to read from the environment sensor
    bool core1_rslt = core1_setup();
    multicore_launch_core1(core1_main);

    // enable events by default
    app_regs.enable_sensor_dispatch_events = 1;
    app_regs.temperature_offset_c = TEMPERATURE_OFFSET_C;

    while(true) {

        app.run();
        
    }
}

static void configure_clock()
{
    clock_configure(clk_sys,
                    CLOCKS_CLK_SYS_CTRL_SRC_VALUE_CLKSRC_CLK_SYS_AUX,
                    CLOCKS_CLK_SYS_CTRL_AUXSRC_VALUE_CLKSRC_PLL_USB,
                    SYS_CLOCK_SPEED_MHZ * MHZ,
                    SYS_CLOCK_SPEED_MHZ * MHZ);

    // Turn off PLL sys for good measure
    pll_deinit(pll_sys);

    // CLK peri is clocked from clk_sys so need to change clk_peri's freq
    clock_configure(clk_peri,
                    CLOCKS_CLK_PERI_CTRL_AUXSRC_VALUE_CLKSRC_PLL_USB,
                    CLOCKS_CLK_PERI_CTRL_AUXSRC_VALUE_CLK_SYS,
                    clock_get_hz(clk_usb),
                    clock_get_hz(clk_usb));

    // CLK ref is clocked from clk_sys so need to change clk_peri's freq
    clock_configure(clk_ref,
                    CLOCKS_CLK_REF_CTRL_SRC_VALUE_XOSC_CLKSRC,
                    CLOCKS_CLK_REF_CTRL_SRC_VALUE_CLKSRC_CLK_REF_AUX,
                    SYS_CLOCK_SPEED_MHZ * MHZ,
                    SYS_CLOCK_SPEED_MHZ * MHZ);

    // Re init uart now that clk_peri has changed
    stdio_init_all();
    
}
