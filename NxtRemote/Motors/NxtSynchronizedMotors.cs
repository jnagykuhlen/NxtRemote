namespace NxtRemote.Motors;

public class NxtSynchronizedMotors(NxtMotor firstMotor, NxtMotor secondMotor)
{
    public void Run(float power, float turnRatio, int tachoLimit = 0) =>
        NxtMotor.RunSynchronized(firstMotor, secondMotor, power, turnRatio, tachoLimit);

    public Task RunAsync(float power, float turnRatio, int tachoLimit) =>
        NxtMotor.RunSynchronizedAsync(firstMotor, secondMotor, power, turnRatio, tachoLimit);

    public void Break()
    {
        firstMotor.Break();
        secondMotor.Break();
    }

    public void Coast()
    {
        firstMotor.Coast();
        secondMotor.Coast();
    }
}
