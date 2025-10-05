namespace NxtRemote.Motors;

[Flags]
public enum NxtMotorRunState : byte
{
    Idle = 0x00,
    RampUp = 0x10,
    Running = 0x20,
    RampDown = 0x40
}