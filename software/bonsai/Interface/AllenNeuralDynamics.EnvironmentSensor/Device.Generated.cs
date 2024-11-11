using Bonsai;
using Bonsai.Harp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace AllenNeuralDynamics.EnvironmentSensor
{
    /// <summary>
    /// Generates events and processes commands for the EnvironmentSensor device connected
    /// at the specified serial port.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Generates events and processes commands for the EnvironmentSensor device.")]
    public partial class Device : Bonsai.Harp.Device, INamedElement
    {
        /// <summary>
        /// Represents the unique identity class of the <see cref="EnvironmentSensor"/> device.
        /// This field is constant.
        /// </summary>
        public const int WhoAmI = 1405;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device() : base(WhoAmI) { }

        string INamedElement.Name => nameof(EnvironmentSensor);

        /// <summary>
        /// Gets a read-only mapping from address to register type.
        /// </summary>
        public static new IReadOnlyDictionary<int, Type> RegisterMap { get; } = new Dictionary<int, Type>
            (Bonsai.Harp.Device.RegisterMap.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            { 32, typeof(Pressure) },
            { 33, typeof(Temperature) },
            { 34, typeof(Humidity) },
            { 35, typeof(SensorData) },
            { 36, typeof(EnableEvents) }
        };

        /// <summary>
        /// Gets the contents of the metadata file describing the <see cref="EnvironmentSensor"/>
        /// device registers.
        /// </summary>
        public static readonly string Metadata = GetDeviceMetadata();

        static string GetDeviceMetadata()
        {
            var deviceType = typeof(Device);
            using var metadataStream = deviceType.Assembly.GetManifestResourceStream($"{deviceType.Namespace}.device.yml");
            using var streamReader = new System.IO.StreamReader(metadataStream);
            return streamReader.ReadToEnd();
        }
    }

    /// <summary>
    /// Represents an operator that returns the contents of the metadata file
    /// describing the <see cref="EnvironmentSensor"/> device registers.
    /// </summary>
    [Description("Returns the contents of the metadata file describing the EnvironmentSensor device registers.")]
    public partial class GetMetadata : Source<string>
    {
        /// <summary>
        /// Returns an observable sequence with the contents of the metadata file
        /// describing the <see cref="EnvironmentSensor"/> device registers.
        /// </summary>
        /// <returns>
        /// A sequence with a single <see cref="string"/> object representing the
        /// contents of the metadata file.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Return(Device.Metadata);
        }
    }

    /// <summary>
    /// Represents an operator that groups the sequence of <see cref="EnvironmentSensor"/>" messages by register type.
    /// </summary>
    [Description("Groups the sequence of EnvironmentSensor messages by register type.")]
    public partial class GroupByRegister : Combinator<HarpMessage, IGroupedObservable<Type, HarpMessage>>
    {
        /// <summary>
        /// Groups an observable sequence of <see cref="EnvironmentSensor"/> messages
        /// by register type.
        /// </summary>
        /// <param name="source">The sequence of Harp device messages.</param>
        /// <returns>
        /// A sequence of observable groups, each of which corresponds to a unique
        /// <see cref="EnvironmentSensor"/> register.
        /// </returns>
        public override IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<HarpMessage> source)
        {
            return source.GroupBy(message => Device.RegisterMap[message.Address]);
        }
    }

    /// <summary>
    /// Represents an operator that filters register-specific messages
    /// reported by the <see cref="EnvironmentSensor"/> device.
    /// </summary>
    /// <seealso cref="Pressure"/>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="Humidity"/>
    /// <seealso cref="SensorData"/>
    /// <seealso cref="EnableEvents"/>
    [XmlInclude(typeof(Pressure))]
    [XmlInclude(typeof(Temperature))]
    [XmlInclude(typeof(Humidity))]
    [XmlInclude(typeof(SensorData))]
    [XmlInclude(typeof(EnableEvents))]
    [Description("Filters register-specific messages reported by the EnvironmentSensor device.")]
    public class FilterRegister : FilterRegisterBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterRegister"/> class.
        /// </summary>
        public FilterRegister()
        {
            Register = new Pressure();
        }

        string INamedElement.Name
        {
            get => $"{nameof(EnvironmentSensor)}.{GetElementDisplayName(Register)}";
        }
    }

    /// <summary>
    /// Represents an operator which filters and selects specific messages
    /// reported by the EnvironmentSensor device.
    /// </summary>
    /// <seealso cref="Pressure"/>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="Humidity"/>
    /// <seealso cref="SensorData"/>
    /// <seealso cref="EnableEvents"/>
    [XmlInclude(typeof(Pressure))]
    [XmlInclude(typeof(Temperature))]
    [XmlInclude(typeof(Humidity))]
    [XmlInclude(typeof(SensorData))]
    [XmlInclude(typeof(EnableEvents))]
    [XmlInclude(typeof(TimestampedPressure))]
    [XmlInclude(typeof(TimestampedTemperature))]
    [XmlInclude(typeof(TimestampedHumidity))]
    [XmlInclude(typeof(TimestampedSensorData))]
    [XmlInclude(typeof(TimestampedEnableEvents))]
    [Description("Filters and selects specific messages reported by the EnvironmentSensor device.")]
    public partial class Parse : ParseBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parse"/> class.
        /// </summary>
        public Parse()
        {
            Register = new Pressure();
        }

        string INamedElement.Name => $"{nameof(EnvironmentSensor)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents an operator which formats a sequence of values as specific
    /// EnvironmentSensor register messages.
    /// </summary>
    /// <seealso cref="Pressure"/>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="Humidity"/>
    /// <seealso cref="SensorData"/>
    /// <seealso cref="EnableEvents"/>
    [XmlInclude(typeof(Pressure))]
    [XmlInclude(typeof(Temperature))]
    [XmlInclude(typeof(Humidity))]
    [XmlInclude(typeof(SensorData))]
    [XmlInclude(typeof(EnableEvents))]
    [Description("Formats a sequence of values as specific EnvironmentSensor register messages.")]
    public partial class Format : FormatBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Format"/> class.
        /// </summary>
        public Format()
        {
            Register = new Pressure();
        }

        string INamedElement.Name => $"{nameof(EnvironmentSensor)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents a register that pressure, in Pa.
    /// </summary>
    [Description("Pressure, in Pa")]
    public partial class Pressure
    {
        /// <summary>
        /// Represents the address of the <see cref="Pressure"/> register. This field is constant.
        /// </summary>
        public const int Address = 32;

        /// <summary>
        /// Represents the payload type of the <see cref="Pressure"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U32;

        /// <summary>
        /// Represents the length of the <see cref="Pressure"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Pressure"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static uint GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt32();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Pressure"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<uint> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt32();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Pressure"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Pressure"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, uint value)
        {
            return HarpMessage.FromUInt32(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Pressure"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Pressure"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, uint value)
        {
            return HarpMessage.FromUInt32(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Pressure register.
    /// </summary>
    /// <seealso cref="Pressure"/>
    [Description("Filters and selects timestamped messages from the Pressure register.")]
    public partial class TimestampedPressure
    {
        /// <summary>
        /// Represents the address of the <see cref="Pressure"/> register. This field is constant.
        /// </summary>
        public const int Address = Pressure.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Pressure"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<uint> GetPayload(HarpMessage message)
        {
            return Pressure.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that temperature in degrees C.
    /// </summary>
    [Description("Temperature in degrees C")]
    public partial class Temperature
    {
        /// <summary>
        /// Represents the address of the <see cref="Temperature"/> register. This field is constant.
        /// </summary>
        public const int Address = 33;

        /// <summary>
        /// Represents the payload type of the <see cref="Temperature"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Temperature"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Temperature"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Temperature"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Temperature"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Temperature"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Temperature"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Temperature"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Temperature register.
    /// </summary>
    /// <seealso cref="Temperature"/>
    [Description("Filters and selects timestamped messages from the Temperature register.")]
    public partial class TimestampedTemperature
    {
        /// <summary>
        /// Represents the address of the <see cref="Temperature"/> register. This field is constant.
        /// </summary>
        public const int Address = Temperature.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Temperature"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Temperature.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that humidity, in %RH.
    /// </summary>
    [Description("Humidity, in %RH")]
    public partial class Humidity
    {
        /// <summary>
        /// Represents the address of the <see cref="Humidity"/> register. This field is constant.
        /// </summary>
        public const int Address = 34;

        /// <summary>
        /// Represents the payload type of the <see cref="Humidity"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Humidity"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Humidity"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Humidity"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Humidity"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Humidity"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Humidity"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Humidity"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Humidity register.
    /// </summary>
    /// <seealso cref="Humidity"/>
    [Description("Filters and selects timestamped messages from the Humidity register.")]
    public partial class TimestampedHumidity
    {
        /// <summary>
        /// Represents the address of the <see cref="Humidity"/> register. This field is constant.
        /// </summary>
        public const int Address = Humidity.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Humidity"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Humidity.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that a periodic event will be emitted with aggregated data from all sensors.
    /// </summary>
    [Description("A periodic event will be emitted with aggregated data from all sensors.")]
    public partial class SensorData
    {
        /// <summary>
        /// Represents the address of the <see cref="SensorData"/> register. This field is constant.
        /// </summary>
        public const int Address = 35;

        /// <summary>
        /// Represents the payload type of the <see cref="SensorData"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="SensorData"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 3;

        static SensorDataPayload ParsePayload(float[] payload)
        {
            SensorDataPayload result;
            result.Pressure = payload[0];
            result.Temperature = payload[1];
            result.Humidity = payload[2];
            return result;
        }

        static float[] FormatPayload(SensorDataPayload value)
        {
            float[] result;
            result = new float[3];
            result[0] = value.Pressure;
            result[1] = value.Temperature;
            result[2] = value.Humidity;
            return result;
        }

        /// <summary>
        /// Returns the payload data for <see cref="SensorData"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static SensorDataPayload GetPayload(HarpMessage message)
        {
            return ParsePayload(message.GetPayloadArray<float>());
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="SensorData"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<SensorDataPayload> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadArray<float>();
            return Timestamped.Create(ParsePayload(payload.Value), payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="SensorData"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SensorData"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, SensorDataPayload value)
        {
            return HarpMessage.FromSingle(Address, messageType, FormatPayload(value));
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="SensorData"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SensorData"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, SensorDataPayload value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, FormatPayload(value));
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// SensorData register.
    /// </summary>
    /// <seealso cref="SensorData"/>
    [Description("Filters and selects timestamped messages from the SensorData register.")]
    public partial class TimestampedSensorData
    {
        /// <summary>
        /// Represents the address of the <see cref="SensorData"/> register. This field is constant.
        /// </summary>
        public const int Address = SensorData.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="SensorData"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<SensorDataPayload> GetPayload(HarpMessage message)
        {
            return SensorData.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that enables (~2Hz) or disables the SensorData events.
    /// </summary>
    [Description("Enables (~2Hz) or disables the SensorData events")]
    public partial class EnableEvents
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const int Address = 36;

        /// <summary>
        /// Represents the payload type of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="EnableEvents"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static Events GetPayload(HarpMessage message)
        {
            return (Events)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="EnableEvents"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Events> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((Events)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="EnableEvents"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableEvents"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, Events value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="EnableEvents"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableEvents"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, Events value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// EnableEvents register.
    /// </summary>
    /// <seealso cref="EnableEvents"/>
    [Description("Filters and selects timestamped messages from the EnableEvents register.")]
    public partial class TimestampedEnableEvents
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const int Address = EnableEvents.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="EnableEvents"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<Events> GetPayload(HarpMessage message)
        {
            return EnableEvents.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents an operator which creates standard message payloads for the
    /// EnvironmentSensor device.
    /// </summary>
    /// <seealso cref="CreatePressurePayload"/>
    /// <seealso cref="CreateTemperaturePayload"/>
    /// <seealso cref="CreateHumidityPayload"/>
    /// <seealso cref="CreateSensorDataPayload"/>
    /// <seealso cref="CreateEnableEventsPayload"/>
    [XmlInclude(typeof(CreatePressurePayload))]
    [XmlInclude(typeof(CreateTemperaturePayload))]
    [XmlInclude(typeof(CreateHumidityPayload))]
    [XmlInclude(typeof(CreateSensorDataPayload))]
    [XmlInclude(typeof(CreateEnableEventsPayload))]
    [XmlInclude(typeof(CreateTimestampedPressurePayload))]
    [XmlInclude(typeof(CreateTimestampedTemperaturePayload))]
    [XmlInclude(typeof(CreateTimestampedHumidityPayload))]
    [XmlInclude(typeof(CreateTimestampedSensorDataPayload))]
    [XmlInclude(typeof(CreateTimestampedEnableEventsPayload))]
    [Description("Creates standard message payloads for the EnvironmentSensor device.")]
    public partial class CreateMessage : CreateMessageBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMessage"/> class.
        /// </summary>
        public CreateMessage()
        {
            Payload = new CreatePressurePayload();
        }

        string INamedElement.Name => $"{nameof(EnvironmentSensor)}.{GetElementDisplayName(Payload)}";
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that pressure, in Pa.
    /// </summary>
    [DisplayName("PressurePayload")]
    [Description("Creates a message payload that pressure, in Pa.")]
    public partial class CreatePressurePayload
    {
        /// <summary>
        /// Gets or sets the value that pressure, in Pa.
        /// </summary>
        [Description("The value that pressure, in Pa.")]
        public uint Pressure { get; set; }

        /// <summary>
        /// Creates a message payload for the Pressure register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public uint GetPayload()
        {
            return Pressure;
        }

        /// <summary>
        /// Creates a message that pressure, in Pa.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Pressure register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.Pressure.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that pressure, in Pa.
    /// </summary>
    [DisplayName("TimestampedPressurePayload")]
    [Description("Creates a timestamped message payload that pressure, in Pa.")]
    public partial class CreateTimestampedPressurePayload : CreatePressurePayload
    {
        /// <summary>
        /// Creates a timestamped message that pressure, in Pa.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Pressure register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.Pressure.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that temperature in degrees C.
    /// </summary>
    [DisplayName("TemperaturePayload")]
    [Description("Creates a message payload that temperature in degrees C.")]
    public partial class CreateTemperaturePayload
    {
        /// <summary>
        /// Gets or sets the value that temperature in degrees C.
        /// </summary>
        [Description("The value that temperature in degrees C.")]
        public float Temperature { get; set; }

        /// <summary>
        /// Creates a message payload for the Temperature register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Temperature;
        }

        /// <summary>
        /// Creates a message that temperature in degrees C.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Temperature register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.Temperature.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that temperature in degrees C.
    /// </summary>
    [DisplayName("TimestampedTemperaturePayload")]
    [Description("Creates a timestamped message payload that temperature in degrees C.")]
    public partial class CreateTimestampedTemperaturePayload : CreateTemperaturePayload
    {
        /// <summary>
        /// Creates a timestamped message that temperature in degrees C.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Temperature register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.Temperature.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that humidity, in %RH.
    /// </summary>
    [DisplayName("HumidityPayload")]
    [Description("Creates a message payload that humidity, in %RH.")]
    public partial class CreateHumidityPayload
    {
        /// <summary>
        /// Gets or sets the value that humidity, in %RH.
        /// </summary>
        [Description("The value that humidity, in %RH.")]
        public float Humidity { get; set; }

        /// <summary>
        /// Creates a message payload for the Humidity register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Humidity;
        }

        /// <summary>
        /// Creates a message that humidity, in %RH.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Humidity register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.Humidity.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that humidity, in %RH.
    /// </summary>
    [DisplayName("TimestampedHumidityPayload")]
    [Description("Creates a timestamped message payload that humidity, in %RH.")]
    public partial class CreateTimestampedHumidityPayload : CreateHumidityPayload
    {
        /// <summary>
        /// Creates a timestamped message that humidity, in %RH.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Humidity register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.Humidity.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that a periodic event will be emitted with aggregated data from all sensors.
    /// </summary>
    [DisplayName("SensorDataPayload")]
    [Description("Creates a message payload that a periodic event will be emitted with aggregated data from all sensors.")]
    public partial class CreateSensorDataPayload
    {
        /// <summary>
        /// Gets or sets a value that pressure, in Pa.
        /// </summary>
        [Description("Pressure, in Pa")]
        public float Pressure { get; set; }

        /// <summary>
        /// Gets or sets a value that temperature in degrees C.
        /// </summary>
        [Description("Temperature in degrees C")]
        public float Temperature { get; set; }

        /// <summary>
        /// Gets or sets a value that humidity, in %RH.
        /// </summary>
        [Description("Humidity, in %RH")]
        public float Humidity { get; set; }

        /// <summary>
        /// Creates a message payload for the SensorData register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public SensorDataPayload GetPayload()
        {
            SensorDataPayload value;
            value.Pressure = Pressure;
            value.Temperature = Temperature;
            value.Humidity = Humidity;
            return value;
        }

        /// <summary>
        /// Creates a message that a periodic event will be emitted with aggregated data from all sensors.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the SensorData register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.SensorData.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that a periodic event will be emitted with aggregated data from all sensors.
    /// </summary>
    [DisplayName("TimestampedSensorDataPayload")]
    [Description("Creates a timestamped message payload that a periodic event will be emitted with aggregated data from all sensors.")]
    public partial class CreateTimestampedSensorDataPayload : CreateSensorDataPayload
    {
        /// <summary>
        /// Creates a timestamped message that a periodic event will be emitted with aggregated data from all sensors.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the SensorData register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.SensorData.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that enables (~2Hz) or disables the SensorData events.
    /// </summary>
    [DisplayName("EnableEventsPayload")]
    [Description("Creates a message payload that enables (~2Hz) or disables the SensorData events.")]
    public partial class CreateEnableEventsPayload
    {
        /// <summary>
        /// Gets or sets the value that enables (~2Hz) or disables the SensorData events.
        /// </summary>
        [Description("The value that enables (~2Hz) or disables the SensorData events.")]
        public Events EnableEvents { get; set; }

        /// <summary>
        /// Creates a message payload for the EnableEvents register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public Events GetPayload()
        {
            return EnableEvents;
        }

        /// <summary>
        /// Creates a message that enables (~2Hz) or disables the SensorData events.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the EnableEvents register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.EnableEvents.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that enables (~2Hz) or disables the SensorData events.
    /// </summary>
    [DisplayName("TimestampedEnableEventsPayload")]
    [Description("Creates a timestamped message payload that enables (~2Hz) or disables the SensorData events.")]
    public partial class CreateTimestampedEnableEventsPayload : CreateEnableEventsPayload
    {
        /// <summary>
        /// Creates a timestamped message that enables (~2Hz) or disables the SensorData events.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the EnableEvents register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return AllenNeuralDynamics.EnvironmentSensor.EnableEvents.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents the payload of the SensorData register.
    /// </summary>
    public struct SensorDataPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SensorDataPayload"/> structure.
        /// </summary>
        /// <param name="pressure">Pressure, in Pa</param>
        /// <param name="temperature">Temperature in degrees C</param>
        /// <param name="humidity">Humidity, in %RH</param>
        public SensorDataPayload(
            float pressure,
            float temperature,
            float humidity)
        {
            Pressure = pressure;
            Temperature = temperature;
            Humidity = humidity;
        }

        /// <summary>
        /// Pressure, in Pa
        /// </summary>
        public float Pressure;

        /// <summary>
        /// Temperature in degrees C
        /// </summary>
        public float Temperature;

        /// <summary>
        /// Humidity, in %RH
        /// </summary>
        public float Humidity;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the payload of
        /// the SensorData register.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the payload of the
        /// SensorData register.
        /// </returns>
        public override string ToString()
        {
            return "SensorDataPayload { " +
                "Pressure = " + Pressure + ", " +
                "Temperature = " + Temperature + ", " +
                "Humidity = " + Humidity + " " +
            "}";
        }
    }

    /// <summary>
    /// Available events on the device
    /// </summary>
    [Flags]
    public enum Events : byte
    {
        Disable = 0x0,
        SensorData = 0x1
    }
}
