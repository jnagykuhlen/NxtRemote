namespace NxtRemote.Sensors;

public enum NxtSensorType : byte
{
    None = 0x00,
    Switch = 0x01,
    Temperature = 0x02,
    Reflection = 0x03,
    Angle = 0x04,
    LightActive = 0x05,
    LightInactive = 0x06,
    SoundDb = 0x07,
    SoundDba = 0x08,
    LowSpeed = 0x0A,
    LowSpeed9V = 0x0B,
}
