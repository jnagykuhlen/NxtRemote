namespace NxtRemote.Motors;

public class NxtMotorCommunication(INxtCommunication communication, NxtMotorPort port)
{
    public Task SetOutputStateAsync(float power, NxtMotorMode mode, NxtMotorRegulationMode regulationMode, float turnRatio, NxtMotorRunState runState, int tachoLimit)
    {
        var telegram = new NxtTelegram(12, NxtTelegramType.DirectCommand, NxtCommand.SetOutputState)
            .WriteByte((byte)port)
            .WriteByte((byte)Denormalize(power))
            .WriteByte((byte)mode)
            .WriteByte((byte)regulationMode)
            .WriteByte((byte)Denormalize(turnRatio))
            .WriteByte((byte)runState)
            .WriteUInt32((uint)tachoLimit);

        return communication.SendWithoutReplyAsync(telegram);
    }

    public async Task<NxtMotorOutputState> GetOutputStateAsync()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.GetOutputState)
            .WriteByte((byte)port);

        var reply = await communication.SendWithReplyAsync(telegram);
        var replyTelegram = reply.Success();

        if ((NxtMotorPort)replyTelegram.ReadByte() != port)
            throw new NxtCommunicationException("Reply motor port does not match requested motor port.");

        return new NxtMotorOutputState(
            Normalize((sbyte)replyTelegram.ReadByte()),
            (NxtMotorMode)replyTelegram.ReadByte(),
            (NxtMotorRegulationMode)replyTelegram.ReadByte(),
            Normalize((sbyte)replyTelegram.ReadByte()),
            (NxtMotorRunState)replyTelegram.ReadByte(),
            (int)replyTelegram.ReadUInt32(),
            replyTelegram.ReadInt32(),
            replyTelegram.ReadInt32(),
            replyTelegram.ReadInt32()
        );
    }

    public Task ResetMotorPositionAsync(NxtResetMotorMode mode)
    {
        var telegram = new NxtTelegram(4, NxtTelegramType.DirectCommand, NxtCommand.ResetMotorPosition)
            .WriteByte((byte)port)
            .WriteByte((byte)mode);
        
        return communication.SendWithoutReplyAsync(telegram);
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
