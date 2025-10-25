namespace NxtRemote.Sensors;

public class NxtLightSensor(NxtSensorCommunication communication, TimeSpan pollingInterval, bool lightActive) : AnalogSensor<float>(
    communication,
    pollingInterval,
    lightActive ? NxtSensorType.LightActive : NxtSensorType.LightInactive,
    NxtSensorMode.PercentageFullScale)
{
    protected override float GetSensorValue(NxtSensorInputValues inputValues) => 0.001f * inputValues.NormalizedValue;
}