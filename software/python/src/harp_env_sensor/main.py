import argparse
import contextlib
import os
from pathlib import Path
from serial.tools import list_ports
import sys
import time
from typing import Optional
import yaml

from pyharp.messages import ReplyHarpMessage
from pyharp.device import DeviceMode, Device

from harp_env_sensor import EnvSensorDevice, __version__

default_yaml = """
%YAML 1.1
---
# yaml-language-server: $schema=https://harp-tech.org/draft-02/schema/device.json
device: EnvironmentSensor
whoAmI: 1405
firmwareVersion: "0.1"
hardwareTargets: "0.1"
registers:
  Pressure:
    access: Read
    address: 32
    type: U32
    description: Pressure, in Pa
  Temperature:
    access: Read
    address: 33
    type: Float
    description: Temperature in degrees C
  Humidity:
    access: Read
    address: 34
    type: Float
    description: Humidity, in %RH
  SensorData:
    access: Event
    address: 35
    type: Float
    description: A periodic event will be emitted with aggregated data from all sensors.
    length: 3
    payloadSpec:
      Pressure:
        offset: 0
        description: Pressure, in Pa
      Temperature:
        offset: 1
        description: Temperature in degrees C
      Humidity:
        offset: 2
        description: Humidity, in %RH
  EnableEvents:
    access: Write
    address: 36
    type: U8
    maskType: Events
    description: Enables (~2Hz) or disables the SensorData events
bitMasks:
  Events:
    bits:
      Disable: 0x0
      SensorData: 0x1
    description: Available events on the device
"""


def read_once(device: EnvSensorDevice) -> tuple[float, float, float]:
    pressure = device.Pressure[0]
    temp = device.Temperature[0]
    humidity = device.Humidity[0]
    print(f"Pressure (Pa): {pressure}, Temperature (C): {temp}, Humidity (prh): {humidity}")
    return pressure, temp, humidity


def read_continuously(device: EnvSensorDevice, file: Optional[Path] = None):
    with contextlib.ExitStack() as stack:
        f = stack.enter_context(open(file, "w")) if file is not None else None
        if f is not None:
            f.write("Timestamp, Pressure_Pa, Temperature_C, Humidity_prh\n")

        while True:
            press, temp, hum = read_once(device)
            if f is not None:
                f.write(f"{time.time()}, {press}, {temp}, {hum}\n")
            time.sleep(1.5)


def read_events(device: EnvSensorDevice, file: Optional[Path] = None):
    with contextlib.ExitStack() as stack:
        f = stack.enter_context(open(file, "w")) if file is not None else None
        if f is not None:
            f.write("Timestamp, Pressure_Pa, Temperature_C, Humidity_prh\n")

        while True:
            reply: ReplyHarpMessage = device._ser.event_q.get()

            timestamp = reply.timestamp
            pressure, temp, humidity = reply.payload
            print(f"Timestamp: {timestamp}, Pressure: {pressure}, Temperature: {temp}, Humidity: {humidity}")
            if f is not None:
                f.write(f"{timestamp}, {pressure}, {temp}, {humidity}\n")


def cli() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Environment Sensor Harp Device",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )

    parser.add_argument("-v", "--version", action="version", version=__version__)
    parser.add_argument("--device-yaml", type=os.path.abspath, default=None)
    parser.add_argument("--comport", type=str, help="Serial port where the device is connected", default=None)
    parser.add_argument("--file", type=os.path.abspath)
    parser.add_argument("--mode", choices=["single", "continuous", "events"], default="single")
    args = parser.parse_args(sys.argv[1:])

    return args


if __name__ == "__main__":

    args = cli()

    if args.comport is None:
        ports = [port.name for port in list_ports.comports()]
        print("Available COM ports:\n")
        print("\n".join(ports) + "\n")
        args.comport = port = input("Which COM port is your device on?")

    if args.device_yaml is None:
        device_yaml = yaml.safe_load(default_yaml)
    else:
        # device_yaml_path = Path(__file__).parents[4] / "device.yml"
        with open(args.device_yaml, "r") as file:
            device_yaml = yaml.safe_load(file)

    print(device_yaml["firmwareVersion"])

    config_dict = {
        "dump_file_path": None,
        "device_configuration": device_yaml,
    }

    device = EnvSensorDevice(args.comport, config_dict)
    device.set_mode(DeviceMode.Active)

    if args.mode == "single":
        read_once(device)
    elif args.mode == "continuous":
        read_continuously(device, args.file)
    elif args.mode == "events":
        read_events(device, args.file)
