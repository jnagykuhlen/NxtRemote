namespace NxtRemote.Sensors;

public class NxtTouchSensor
{
    public IPollable<TouchState> Pollable { get; }

    public NxtTouchSensor(NxtSensorCommunication communication, TimeSpan pollingInterval)
    {
        communication.SetInputMode(NxtSensorType.Switch, NxtSensorMode.Boolean);
        
        Pollable = new Polling<TouchState>(
            () => communication.GetInputValues().ScaledValue > 0 ? TouchState.Pressed : TouchState.Unpressed,
            pollingInterval
        );
    }

    public TouchState GetState() => Pollable.PollOnce();
    public Task WaitForStateAsync(TouchState desiredState) => Pollable.WhenAsync(state => state == desiredState);
}

public enum TouchState
{
    Unpressed = 0,
    Pressed = 1
}
