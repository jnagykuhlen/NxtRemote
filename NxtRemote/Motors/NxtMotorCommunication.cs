namespace NxtRemote.Motors;

public class NxtMotorCommunication(INxtCommunication communication, NxtMotorPort port)
{
    public void SetOutputState(sbyte power, NxtMotorMode mode, NxtMotorRegulationMode regulationMode, sbyte turnRatio, NxtMotorRunState runState, uint tachoLimit)
    {
        var telegram = new NxtTelegram(12, NxtTelegramType.DirectCommand, NxtCommand.SetOutputState)
            .WriteByte((byte)port)
            .WriteByte((byte)sbyte.Clamp(power, -100, 100))
            .WriteByte((byte)mode)
            .WriteByte((byte)regulationMode)
            .WriteByte((byte)sbyte.Clamp(turnRatio, -100, 100))
            .WriteByte((byte)runState)
            .WriteUInt32(tachoLimit);

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
            (sbyte)reply.ReadByte(),
            (NxtMotorMode)reply.ReadByte(),
            (NxtMotorRegulationMode)reply.ReadByte(),
            (sbyte)reply.ReadByte(),
            (NxtMotorRunState)reply.ReadByte(),
            reply.ReadUInt32(),
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
}

public record struct NxtMotorOutputState(
    sbyte Power,
    NxtMotorMode Mode,
    NxtMotorRegulationMode RegulationMode,
    sbyte TurnRatio,
    NxtMotorRunState RunState,
    uint TachoLimit,
    int TachoCount,
    int BlockTachoCount,
    int RotationCount
);
