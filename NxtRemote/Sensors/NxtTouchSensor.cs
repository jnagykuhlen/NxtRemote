namespace NxtRemote.Sensors;

public class NxtTouchSensor(NxtSensorCommunication communication, TimeSpan pollingInterval)
{
    protected IPollable<NxtSensorInputValues> Pollable { get; } =
        new Polling<NxtSensorInputValues>(communication.GetInputValues, pollingInterval);
    
    public TouchState GetState() => GetTouchState(communication.GetInputValues());
    
    public Task WaitForStateAsync(TouchState state) =>
        Pollable.WhenAsync(inputValues => GetTouchState(inputValues) == state);
    
    private static TouchState GetTouchState(NxtSensorInputValues inputValues) =>
        inputValues.ScaledValue > 0 ? TouchState.Pressed : TouchState.Unpressed;
}

public enum TouchState
{
    Unpressed = 0,
    Pressed = 1
}
