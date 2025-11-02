namespace NxtRemote.Sensors;

public class NxtSensorCommunication(INxtCommunication communication, NxtSensorPort port)
{
    public NxtSensorInputValues GetInputValues()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.GetInputValues)
            .WriteByte((byte)port);

        var replyTelegram = communication.SendWithReply(telegram).Success();

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

    public void SetInputMode(NxtSensorType sensorType, NxtSensorMode sensorMode)
    {
        var telegram = new NxtTelegram(5, NxtTelegramType.DirectCommand, NxtCommand.SetInputMode)
            .WriteByte((byte)port)
            .WriteByte((byte)sensorType)
            .WriteByte((byte)sensorMode);

        communication.SendWithoutReply(telegram);
    }

    public byte DigitalReadByte(byte address) => DigitalRead(address, 1).ReadByte();
    public string DigitalReadString(byte address, byte maxLength) =>
        DigitalRead(address, maxLength).ReadString(maxLength);

    private NxtTelegram DigitalRead(byte address, byte expectedResponseLength)
    {
        LowSpeedWrite(address, expectedResponseLength);

        byte bytesReady;
        do
        {
            bytesReady = LowSpeedGetStatus();
        } while (bytesReady < expectedResponseLength);

        return LowSpeedRead(expectedResponseLength);
    }

    private NxtTelegram LowSpeedRead(byte expectedResponseLength)
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.LowSpeedRead)
            .WriteByte((byte)port);

        var replyTelegram = communication.SendWithReply(telegram).Success();

        var responseLength = replyTelegram.ReadByte();
        if (responseLength < expectedResponseLength)
            throw new NxtCommunicationException($"Only read {responseLength} bytes instead of {expectedResponseLength}.");

        return replyTelegram;
    }

    private void LowSpeedWrite(byte address, byte expectedResponseLength)
    {
        var telegram = new NxtTelegram(7, NxtTelegramType.DirectCommand, NxtCommand.LowSpeedWrite)
            .WriteByte((byte)port)
            .WriteByte(2)
            .WriteByte(expectedResponseLength)
            .WriteByte(0x02)
            .WriteByte(address);

        communication.SendWithoutReply(telegram);
    }

    private byte LowSpeedGetStatus()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.LowSpeedGetStatus)
            .WriteByte((byte)port);

        var reply = communication.SendWithReply(telegram);
        if (reply.Status == NxtCommandStatus.PendingCommunicationTransactionInProgress)
            return 0;

        var replyTelegram = reply.Success();
        return replyTelegram.ReadByte();
    }
}