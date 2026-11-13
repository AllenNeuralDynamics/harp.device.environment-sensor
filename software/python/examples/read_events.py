"""Enable the periodic SensorData event, print incoming events and optionally save them to a csv file."""

import argparse
import contextlib
from pathlib import Path
from time import sleep

from harp.device import environment_sensor as env
from harp.protocol import HarpMessage
from harp.serial import open_device


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", required=True, help="Serial port of the device.")
    parser.add_argument("--file", type=Path, default=None, help="Optional csv output file.")
    args = parser.parse_args()

    with contextlib.ExitStack() as stack:
        dev = stack.enter_context(open_device(env, port=args.port))
        f = stack.enter_context(open(args.file, "w")) if args.file else None
        if f:
            f.write("Timestamp,Pressure_Pa,Temperature_C,Humidity_prh\n")

        def on_event(msg: HarpMessage) -> None:
            p = msg.payload
            print(f"Timestamp: {msg.timestamp}, Pressure: {p.pressure}, Temperature: {p.temperature}, Humidity: {p.humidity}")
            if f:
                f.write(f"{msg.timestamp},{p.pressure},{p.temperature},{p.humidity}\n")
                f.flush()

        with dev.subscribe(env.SensorData, on_event):
            dev.write(env.EnableEvents, env.Events.SENSOR_DATA)
            try:
                while True:
                    sleep(0.1)
            except KeyboardInterrupt:
                pass
            finally:
                dev.write(env.EnableEvents, env.Events(0))


if __name__ == "__main__":
    main()
