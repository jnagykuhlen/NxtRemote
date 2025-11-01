namespace NxtRemote.Sensors;

public class NxtSensorCommunication(INxtCommunication communication, NxtSensorPort port)
{
    public NxtSensorInputValues GetInputValues()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.GetInputValues)
            .WriteByte((byte)port);

        var reply = communication.SendWithReply(telegram);
        
        var replyPort = (NxtSensorPort)reply.ReadByte();
        if (replyPort != port)
            throw new NxtCommunicationException($"Reply sensor port \"{replyPort}\" does not match requested sensor port \"{port}\".");

        return new NxtSensorInputValues(
            reply.ReadByte() > 0,
            reply.ReadByte() > 0,
            (NxtSensorType)reply.ReadByte(),
            (NxtSensorMode)reply.ReadByte(),
            reply.ReadUInt16(),
            reply.ReadUInt16(),
            reply.ReadUInt16(),
            reply.ReadUInt16()
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