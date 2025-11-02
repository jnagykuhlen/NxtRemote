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
}