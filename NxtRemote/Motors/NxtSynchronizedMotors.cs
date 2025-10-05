namespace NxtRemote.Motors;

public class NxtSynchronizedMotors(NxtMotor firstMotor, NxtMotor secondMotor)
{
    public void Run(float power, float turnRatio, int tachoLimit = 0)
    {
        if (tachoLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(tachoLimit));

        NxtMotor.RunSynchronized(firstMotor, secondMotor, power, turnRatio, tachoLimit);
    }

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
