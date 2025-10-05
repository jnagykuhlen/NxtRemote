namespace NxtRemote.Motors;

[Flags]
public enum NxtMotorMode : byte
{
    Off = 0x00,
    On = 0x01,
    Brake = 0x02,
    Regulated = 0x04
}
