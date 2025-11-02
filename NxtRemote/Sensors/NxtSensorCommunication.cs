namespace NxtRemote.Sensors;

public class NxtSensorCommunication(INxtCommunication communication, NxtSensorPort port)
{
    public async Task<NxtSensorInputValues> GetInputValuesAsync()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.GetInputValues)
            .WriteByte((byte)port);

        var reply = await communication.SendWithReplyAsync(telegram);
        var replyTelegram = reply.Success();

        var replyPort = (NxtSensorPort)replyTelegram.ReadByte();
        if (replyPort != port)
            throw new NxtCommunicationException($"Reply sensor port \"{replyPort}\" does not match requested sensor port \"{port}\".");

        return new NxtSensorInputValues(
            replyTelegram.ReadByte() > 0,
            replyTelegram.ReadByte() > 0,
            (NxtSensorType)replyTelegram.ReadByte(),
            (NxtSensorMode)replyTelegram.ReadByte(),
            replyTelegram.ReadUInt16(),
            replyTelegram.ReadUInt16(),
            replyTelegram.ReadUInt16(),
            replyTelegram.ReadUInt16()
        );
    }

    public Task SetInputModeAsync(NxtSensorType sensorType, NxtSensorMode sensorMode)
    {
        var telegram = new NxtTelegram(5, NxtTelegramType.DirectCommand, NxtCommand.SetInputMode)
            .WriteByte((byte)port)
            .WriteByte((byte)sensorType)
            .WriteByte((byte)sensorMode);

        return communication.SendWithoutReplyAsync(telegram);
    }

    public async Task<byte> DigitalReadByteAsync(byte address) => (await DigitalReadAsync(address, 1)).ReadByte();
    public async Task<string> DigitalReadStringAsync(byte address, byte maxLength) =>
        (await DigitalReadAsync(address, maxLength)).ReadString(maxLength);

    private async Task<NxtTelegram> DigitalReadAsync(byte address, byte expectedResponseLength)
    {
        await LowSpeedWriteAsync(address, expectedResponseLength);

        byte bytesReady;
        do
        {
            bytesReady = await LowSpeedGetStatusAsync();
        } while (bytesReady < expectedResponseLength);

        return await LowSpeedReadAsync(expectedResponseLength);
    }

    private async Task<NxtTelegram> LowSpeedReadAsync(byte expectedResponseLength)
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.LowSpeedRead)
            .WriteByte((byte)port);

        var reply = await communication.SendWithReplyAsync(telegram);
        var replyTelegram = reply.Success();

        var responseLength = replyTelegram.ReadByte();
        if (responseLength < expectedResponseLength)
            throw new NxtCommunicationException($"Only read {responseLength} bytes instead of {expectedResponseLength}.");

        return replyTelegram;
    }

    private Task LowSpeedWriteAsync(byte address, byte expectedResponseLength)
    {
        var telegram = new NxtTelegram(7, NxtTelegramType.DirectCommand, NxtCommand.LowSpeedWrite)
            .WriteByte((byte)port)
            .WriteByte(2)
            .WriteByte(expectedResponseLength)
            .WriteByte(0x02)
            .WriteByte(address);

        return communication.SendWithoutReplyAsync(telegram);
    }

    private async Task<byte> LowSpeedGetStatusAsync()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.LowSpeedGetStatus)
            .WriteByte((byte)port);

        var reply = await communication.SendWithReplyAsync(telegram);
        if (reply.Status == NxtCommandStatus.PendingCommunicationTransactionInProgress)
            return 0;

        var replyTelegram = reply.Success();
        return replyTelegram.ReadByte();
    }
}