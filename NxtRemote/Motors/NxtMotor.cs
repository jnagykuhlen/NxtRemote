namespace NxtRemote.Motors;

public class NxtMotor(NxtMotorCommunication communication)
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(50);

    private readonly Polling<NxtMotorOutputState> polling = new(communication.GetOutputState, PollingInterval);

    public void Run(float power, int tachoLimit = 0)
    {
        if (tachoLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(tachoLimit));

        SetOutputState(
            power,
            NxtMotorMode.On | NxtMotorMode.Regulated,
            NxtMotorRegulationMode.Speed,
            0,
            NxtMotorRunState.Running,
            tachoLimit
        );
    }

    public Task RunAsync(float power, int tachoLimit)
    {
        if (tachoLimit == 0)
            throw new InvalidOperationException("Tacho limit must be specified in order to await completion.");

        Run(power, tachoLimit);

        return WaitForIdleAsync();
    }

    public static void RunSynchronized(
        NxtMotor firstMotor,
        NxtMotor secondMotor,
        float power,
        float turnRatio,
        int tachoLimit = 0
    )
    {
        if (tachoLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(tachoLimit));

        firstMotor.SetOutputState(
            power,
            NxtMotorMode.On | NxtMotorMode.Regulated | NxtMotorMode.Brake,
            NxtMotorRegulationMode.Synchronization,
            turnRatio,
            NxtMotorRunState.Running,
            tachoLimit
        );

        secondMotor.SetOutputState(
            power,
            NxtMotorMode.On | NxtMotorMode.Regulated | NxtMotorMode.Brake,
            NxtMotorRegulationMode.Synchronization,
            turnRatio,
            NxtMotorRunState.Running,
            tachoLimit
        );
    }

    public static Task RunSynchronizedAsync(
        NxtMotor firstMotor,
        NxtMotor secondMotor,
        float power,
        float turnRatio,
        int tachoLimit)
    {
        if (tachoLimit == 0)
            throw new InvalidOperationException("Tacho limit must be specified in order to await completion.");

        RunSynchronized(firstMotor, secondMotor, power, turnRatio, tachoLimit);

        return Task.WhenAll(
            firstMotor.WaitForIdleAsync(),
            secondMotor.WaitForIdleAsync()
        );
    }

    public void Coast()
    {
        communication.SetOutputState(
            0,
            NxtMotorMode.On | NxtMotorMode.Regulated,
            NxtMotorRegulationMode.None,
            0,
            NxtMotorRunState.Running,
            0
        );
    }

    public void Break()
    {
        communication.SetOutputState(
            0,
            NxtMotorMode.On | NxtMotorMode.Regulated | NxtMotorMode.Brake,
            NxtMotorRegulationMode.None,
            0,
            NxtMotorRunState.Running,
            0
        );
    }

    private Task WaitForIdleAsync() =>
        polling.WaitForConditionAsync(outputState => outputState.RunState == NxtMotorRunState.Idle);

    private void SetOutputState(
        float power,
        NxtMotorMode mode,
        NxtMotorRegulationMode regulationMode,
        float turnRatio,
        NxtMotorRunState runState,
        int tachoLimit
    )
    {
        communication.SetOutputState(
            (sbyte)(int)(100 * float.Clamp(power, -1.0f, 1.0f)),
            mode,
            regulationMode,
            (sbyte)(int)(100 * float.Clamp(turnRatio, -1.0f, 1.0f)),
            runState,
            (uint)tachoLimit
        );
    }
}