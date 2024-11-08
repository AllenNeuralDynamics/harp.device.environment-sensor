
#include "Arduino.h"

// void Serial::print() {};
// void Serial::absolute_time_min

unsigned long millis()
{
  return to_ms_since_boot(get_absolute_time());
}

void delay(uint32_t ms)
{
  sleep_ms(ms);
}
