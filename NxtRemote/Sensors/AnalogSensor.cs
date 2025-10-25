namespace NxtRemote.Sensors;

public abstract class AnalogSensor<T>
{
    public IPollable<T> Pollable { get; }

    protected AnalogSensor(NxtSensorCommunication communication, TimeSpan pollingInterval, NxtSensorType sensorType, NxtSensorMode sensorMode)
    {
        communication.SetInputMode(sensorType, sensorMode);
        Pollable = new Polling<T>(() => GetSensorValue(communication.GetInputValues()), pollingInterval);
    }

    protected abstract T GetSensorValue(NxtSensorInputValues inputValues);
}