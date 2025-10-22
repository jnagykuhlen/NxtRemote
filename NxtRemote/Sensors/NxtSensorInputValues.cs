namespace NxtRemote.Sensors;

public record struct NxtSensorInputValues(
    bool Calibrated,
    NxtSensorType Type,
    NxtSensorMode Mode,
    ushort RawValue,
    ushort NormalizedValue,
    ushort ScaledValue,
    ushort CalibratedValue
);
