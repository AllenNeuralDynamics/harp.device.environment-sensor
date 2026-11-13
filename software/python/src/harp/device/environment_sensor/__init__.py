# This file was automatically generated and should not be edited directly.
# To make changes, edit the device metadata and regenerate the interface.

import enum
from typing import Any, ClassVar

import numpy as np
from harp.protocol import (
    AnonymousPayload,
    BitMask,
    Field,
    IdentityConverter,
    PayloadType,
    RegisterBase,
    RegisterFloat,
    RegisterU32,
    StructPayload,
)
from harp.device.core import REGISTER_MAP as _CORE_REGISTER_MAP


__all__ = [
    "DEVICE_NAME",
    "WHO_AM_I",
    "Events",
    "SensorDataPayload",
    "EnableEventsPayload",
    "Pressure",
    "Temperature",
    "Humidity",
    "SensorData",
    "EnableEvents",
    "TemperatureOffsetC",
    "REGISTER_MAP",
]

DEVICE_NAME: str = "EnvironmentSensor"
WHO_AM_I: int = 1405


class Events(enum.IntFlag):
    """Available events on the device"""

    SENSOR_DATA = 0x1


class SensorDataPayload(StructPayload[np.float32], length=3):
    """Represents the SensorData payload."""

    pressure: np.float32 = Field(IdentityConverter(np.float32))
    """Pressure, in Pa"""

    temperature: np.float32 = Field(IdentityConverter(np.float32), offset=1)
    """Temperature in degrees C"""

    humidity: np.float32 = Field(IdentityConverter(np.float32), offset=2)
    """Humidity, in %RH"""


class EnableEventsPayload(AnonymousPayload[np.uint8]):
    """Represents the EnableEvents payload."""

    __value__: Events = BitMask(enum=Events)


class Pressure(RegisterU32):
    """Pressure, in Pa"""

    address: ClassVar[int] = 32


class Temperature(RegisterFloat):
    """Temperature in degrees C"""

    address: ClassVar[int] = 33


class Humidity(RegisterFloat):
    """Humidity, in %RH"""

    address: ClassVar[int] = 34


class SensorData(RegisterBase[SensorDataPayload]):
    """A periodic event will be emitted with aggregated data from all sensors."""

    address: ClassVar[int] = 35
    payload_type: ClassVar[PayloadType] = PayloadType.Float
    payload_class = SensorDataPayload


class EnableEvents(RegisterBase[Events]):
    """Enables (~2Hz) or disables the SensorData events"""

    address: ClassVar[int] = 36
    payload_type: ClassVar[PayloadType] = PayloadType.U8
    payload_class = EnableEventsPayload


class TemperatureOffsetC(RegisterFloat):
    """Fixed experimentally-determined calibration offset added to nominal temperature sensor reading in degrees C"""

    address: ClassVar[int] = 37


REGISTER_MAP: dict[int, type[RegisterBase[Any]]] = {
    **_CORE_REGISTER_MAP,
    32: Pressure,
    33: Temperature,
    34: Humidity,
    35: SensorData,
    36: EnableEvents,
    37: TemperatureOffsetC,
}
