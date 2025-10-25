namespace NxtRemote.Sensors;

public class NxtTouchSensor(NxtSensorCommunication communication, TimeSpan pollingInterval) : AnalogSensor<TouchState>(
    communication,
    pollingInterval,
    NxtSensorType.Switch,
    NxtSensorMode.Boolean)
{
    public TouchState GetState() => Pollable.PollOnce();
    public Task WaitForStateAsync(TouchState desiredState) => Pollable.WhenAsync(state => state == desiredState);

    protected override TouchState GetSensorValue(NxtSensorInputValues inputValues) =>
        inputValues.ScaledValue > 0 ? TouchState.Pressed : TouchState.Unpressed;
}

public enum TouchState
{
    Unpressed = 0,
    Pressed = 1
}