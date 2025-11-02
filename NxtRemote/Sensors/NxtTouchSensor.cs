namespace NxtRemote.Sensors;

public class NxtTouchSensor
{
    public IPollable<NxtSensorInputValues> Pollable { get; }

    public NxtTouchSensor(NxtSensorCommunication communication, TimeSpan pollingInterval)
    {
        communication.SetInputModeAsync(NxtSensorType.Switch, NxtSensorMode.Boolean);
        Pollable = new Polling<NxtSensorInputValues>(communication.GetInputValuesAsync, pollingInterval);
    }

    public Task<TouchState> GetTouchStateAsync() => Pollable.NextAsync(GetTouchState);

    public Task<TouchState> WaitForTouchStateAsync(TouchState desiredState) =>
        Pollable.WhenAsync(GetTouchState, state => state == desiredState);

    private static TouchState GetTouchState(NxtSensorInputValues inputValues) =>
        inputValues.ScaledValue > 0 ? TouchState.Pressed : TouchState.Unpressed;
}

public enum TouchState
{
    Unpressed = 0,
    Pressed = 1
}