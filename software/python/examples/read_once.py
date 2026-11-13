"""Take a single reading of pressure, temperature and humidity and print it."""

import argparse

from harp.device import environment_sensor as env
from harp.serial import open_device


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", required=True, help="Serial port of the device.")
    args = parser.parse_args()

    with open_device(env, port=args.port) as dev:
        pressure = dev.read(env.Pressure).payload
        temperature = dev.read(env.Temperature).payload
        humidity = dev.read(env.Humidity).payload
        print(
            f"Pressure (Pa): {pressure}, Temperature (C): {temperature}, "
            f"Humidity (%RH): {humidity}"
        )


if __name__ == "__main__":
    main()
