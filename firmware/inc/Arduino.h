#ifndef ARDUINO_H
#define ARDUINO_H

#include <pico/stdlib.h>

class Serial {
  public:
    void print();
    void println();
};

unsigned long millis();

void delay(uint32_t ms);

void delayMicroseconds(uint32_t ms);
void yield();

#endif // ARDUINO_H