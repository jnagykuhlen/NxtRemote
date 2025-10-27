using NxtRemote.Motors;
using NxtRemote.Sensors;

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

    public NxtMotor GetMotor(NxtMotorPort port, TimeSpan? pollingInterval = null) =>
        new(new NxtMotorCommunication(communication, port), pollingInterval ?? defaultPollingInterval);
    
    public NxtSynchronizedMotors GetSynchronizedMotors(NxtMotorPort firstPort, NxtMotorPort secondPort, TimeSpan? pollingInterval = null) =>
        new(GetMotor(firstPort, pollingInterval ?? defaultPollingInterval), GetMotor(secondPort, pollingInterval ?? defaultPollingInterval));
    
    public NxtTouchSensor GetTouchSensor(NxtSensorPort port, TimeSpan? pollingInterval = null) =>
        new(new NxtSensorCommunication(communication, port), pollingInterval ?? defaultPollingInterval);

    public NxtLightSensor GetLightSensor(NxtSensorPort port, bool lightActive = false, TimeSpan? pollingInterval = null) =>
        new(new NxtSensorCommunication(communication, port), pollingInterval ?? defaultPollingInterval, lightActive);

    public NxtSoundSensor GetSoundSensor(NxtSensorPort port, bool weighted = false, TimeSpan? pollingInterval = null) =>
        new(new NxtSensorCommunication(communication, port), pollingInterval ?? defaultPollingInterval, weighted);
}
