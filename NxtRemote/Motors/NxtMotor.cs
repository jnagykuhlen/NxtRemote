namespace NxtRemote.Motors;

public class NxtMotor(NxtMotorCommunication communication, TimeSpan pollingInterval)
{
    public IPollable<NxtMotorOutputState> Pollable { get; } =
        new Polling<NxtMotorOutputState>(communication.GetOutputStateAsync, pollingInterval);

    protected NxtMotorCommunication Communication { get; } = communication;

    public void Run(float power, int tachoLimit = 0)
    {
        if (tachoLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(tachoLimit));

        Communication.SetOutputStateAsync(
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

        firstMotor.Communication.SetOutputStateAsync(
            power,
            NxtMotorMode.On | NxtMotorMode.Regulated | NxtMotorMode.Brake,
            NxtMotorRegulationMode.Synchronization,
            turnRatio,
            NxtMotorRunState.Running,
            tachoLimit
        );

        secondMotor.Communication.SetOutputStateAsync(
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
        Communication.SetOutputStateAsync(
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
        Communication.SetOutputStateAsync(
            0,
            NxtMotorMode.On | NxtMotorMode.Regulated | NxtMotorMode.Brake,
            NxtMotorRegulationMode.None,
            0,
            NxtMotorRunState.Running,
            0
        );
    }

    public void ResetPosition(NxtResetMotorMode mode) => Communication.ResetMotorPositionAsync(mode);

    private Task WaitForIdleAsync() => Pollable.WhenAsync(outputState => outputState.RunState == NxtMotorRunState.Idle);
}