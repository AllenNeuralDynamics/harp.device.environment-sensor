"""Poll the sensor registers periodically, print the values and optionally save them to a csv file."""

import argparse
import contextlib
import time
from pathlib import Path

from harp.device import environment_sensor as env
from harp.serial import open_device


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", required=True, help="Serial port of the device.")
    parser.add_argument("--file", type=Path, default=None, help="Optional csv output file.")
    parser.add_argument("--update-interval-s", type=float, default=1.5)
    args = parser.parse_args()

    with contextlib.ExitStack() as stack:
        dev = stack.enter_context(open_device(env, port=args.port))
        f = stack.enter_context(open(args.file, "w")) if args.file else None
        if f:
            f.write("Timestamp,Pressure_Pa,Temperature_C,Humidity_prh\n")
        try:
            while True:
                pressure = dev.read(env.Pressure).payload
                temperature = dev.read(env.Temperature).payload
                humidity = dev.read(env.Humidity).payload
                print(f"Pressure (Pa): {pressure}, Temperature (C): {temperature}, Humidity (%RH): {humidity}")
                if f:
                    f.write(f"{time.time()},{pressure},{temperature},{humidity}\n")
                    f.flush()
                time.sleep(args.update_interval_s)
        except KeyboardInterrupt:
            pass


if __name__ == "__main__":
    main()
