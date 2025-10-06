using NxtRemote.Motors;

namespace NxtRemote;

public class NxtController(INxtCommunication communication, TimeSpan defaultPollingInterval)
{
    public NxtController(INxtCommunication communication) :
        this(communication, TimeSpan.FromMilliseconds(100))
    {
    }

    public void PlayTone(int frequency, int durationMilliseconds)
    {
        if (frequency is < 200 or > 1400)
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be between 200 and 1400 Hz.");

        var telegram = new NxtTelegram(6, NxtTelegramType.DirectCommand, NxtCommand.PlayTone)
            .WriteUInt16((ushort)frequency)
            .WriteUInt16((ushort)durationMilliseconds);

        communication.SendWithoutReply(telegram);
    }

    public NxtMotor GetMotor(NxtMotorPort port, TimeSpan pollingInterval) =>
        new(new NxtMotorCommunication(communication, port), pollingInterval);
    
    public NxtMotor GetMotor(NxtMotorPort port) => GetMotor(port, defaultPollingInterval);

    public NxtSynchronizedMotors GetSynchronizedMotors(NxtMotorPort firstPort, NxtMotorPort secondPort, TimeSpan pollingInterval) =>
        new(GetMotor(firstPort, pollingInterval), GetMotor(secondPort, pollingInterval));

    public NxtSynchronizedMotors GetSynchronizedMotors(NxtMotorPort firstPort, NxtMotorPort secondPort) =>
        GetSynchronizedMotors(firstPort, secondPort, defaultPollingInterval);
}