# Python drivers for the environment sensor

Contains a small package to interface with the environment sensor device, and a script runnable with `python -m harp_env_sensor.main` to read the data and optionally save it to a csv file.

## Installation

`pip install "git+https://github.com/AllenNeuralDynamics/harp.device.environment-sensor.git@feat-add-python-script#egg=harp_env_sensor&subdirectory=software/python"`

If you're on a SIPE computer with the default pip environment variables set to the devpi server, you'll need to add `-i https://pypi.org/simple` after the pip install above.

## Usage

CLI Arguments:
- `--comport` Port name where the device is connected. Optional. If missing, it'll list available ports and prompt.
- `--mode` Read mode
    - `single`: Takes one reading, prints it to terminal, and exits
    - `events`: Polls events as they come in and print to terminal and file if provided
    - `continuous`: Reads registers every 1.5 seconds and print to terminal and file if provided
- `--file` Path to file to write output to. Optional.

Examples:

- Read events from COM8 and save to file

    `python -m harp_env_sensor.main --comport COM8 --mode events --file ./output.csv`

- Read the registers regularly and print to terminal

    `python -m harp_env_sensor.main --comport COM8 --mode continuous`