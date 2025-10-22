namespace NxtRemote.Sensors;

public class NxtSensorCommunication(INxtCommunication communication, NxtSensorPort port)
{
    public NxtSensorInputValues GetInputValues()
    {
        var telegram = new NxtTelegram(3, NxtTelegramType.DirectCommand, NxtCommand.GetInputValues)
            .WriteByte((byte)port);

        var reply = communication.SendWithReply(telegram);

        if ((NxtSensorPort)reply.ReadByte() != port)
            throw new NxtCommunicationException("Reply sensor port does not match requested sensor port.");

        if (reply.ReadByte() > 0)
            throw new NxtCommunicationException("Reply sensor input is invalid.");

        return new NxtSensorInputValues(
            reply.ReadByte() > 0,
            (NxtSensorType)reply.ReadByte(),
            (NxtSensorMode)reply.ReadByte(),
            reply.ReadUInt16(),
            reply.ReadUInt16(),
            reply.ReadUInt16(),
            reply.ReadUInt16()
        );
    }
}
