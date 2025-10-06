namespace NxtRemote.Motors;

public class NxtMotorCommunication(INxtCommunication communication, NxtMotorPort port)
{
    public void SetOutputState(float power, NxtMotorMode mode, NxtMotorRegulationMode regulationMode, float turnRatio, NxtMotorRunState runState, int tachoLimit)
    {
        var telegram = new NxtTelegram(12, NxtTelegramType.DirectCommand, NxtCommand.SetOutputState)
            .WriteByte((byte)port)
            .WriteByte((byte)Denormalize(power))
            .WriteByte((byte)mode)
            .WriteByte((byte)regulationMode)
            .WriteByte((byte)Denormalize(turnRatio))
            .WriteByte((byte)runState)
            .WriteUInt32((uint)tachoLimit);

        communication.SendWithoutReply(telegram);
    }

    public NxtMotorOutputState GetOutputState()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.GetOutputState)
            .WriteByte((byte)port);

        var reply = communication.SendWithReply(telegram);

        if ((NxtMotorPort)reply.ReadByte() != port)
            throw new NxtCommunicationException("Reply motor port does not match requested motor port.");

        return new NxtMotorOutputState(
            Normalize((sbyte)reply.ReadByte()),
            (NxtMotorMode)reply.ReadByte(),
            (NxtMotorRegulationMode)reply.ReadByte(),
            Normalize((sbyte)reply.ReadByte()),
            (NxtMotorRunState)reply.ReadByte(),
            (int)reply.ReadUInt32(),
            reply.ReadInt32(),
            reply.ReadInt32(),
            reply.ReadInt32()
        );
    }

    public void ResetMotorPosition(NxtResetMotorMode mode)
    {
        var telegram = new NxtTelegram(4, NxtTelegramType.DirectCommand, NxtCommand.ResetMotorPosition)
            .WriteByte((byte)port)
            .WriteByte((byte)mode);
        
        communication.SendWithoutReply(telegram);
    }
    
    private static sbyte Denormalize(float value) => (sbyte)(int)(100 * float.Clamp(value, -1.0f, 1.0f));
    private static float Normalize(sbyte value) => value / 100.0f;
}

public record struct NxtMotorOutputState(
    float Power,
    NxtMotorMode Mode,
    NxtMotorRegulationMode RegulationMode,
    float TurnRatio,
    NxtMotorRunState RunState,
    int TachoLimit,
    int TachoCount,
    int BlockTachoCount,
    int RotationCount
);
