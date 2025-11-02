namespace NxtRemote.Sensors;

public class NxtLightSensor
{
    public IPollable<NxtSensorInputValues> Pollable { get; }

    public NxtLightSensor(NxtSensorCommunication communication, TimeSpan pollingInterval, bool lightActive)
    {
        communication.SetInputModeAsync(lightActive ? NxtSensorType.LightActive : NxtSensorType.LightInactive, NxtSensorMode.PercentageFullScale);
        Pollable = new Polling<NxtSensorInputValues>(communication.GetInputValuesAsync, pollingInterval);
    }

    public Task<float> GetLightIntensityAsync() => Pollable.NextAsync(GetLightIntensity);

    public Task<float> WaitForLightIntensityAsync(Func<float, bool> predicate) =>
        Pollable.WhenAsync(GetLightIntensity, predicate);

    protected static float GetLightIntensity(NxtSensorInputValues inputValues)
    {
        if (!inputValues.Valid)
            throw new InvalidOperationException("Sensor input values are not valid.");

        return 0.001f * inputValues.NormalizedValue;
    }
}