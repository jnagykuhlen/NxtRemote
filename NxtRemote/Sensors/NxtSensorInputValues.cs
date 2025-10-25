namespace NxtRemote.Sensors;

public record struct NxtSensorInputValues(
    bool Valid,
    bool Calibrated,
    NxtSensorType Type,
    NxtSensorMode Mode,
    ushort RawValue,
    ushort NormalizedValue,
    ushort ScaledValue,
    ushort CalibratedValue
);
