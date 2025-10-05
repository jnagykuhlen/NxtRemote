using NxtRemote.Motors;

namespace NxtRemote;

public class NxtController(INxtCommunication communication)
{
    public NxtMotor GetMotor(NxtMotorPort port) => new(new NxtMotorCommunication(communication, port));
    
    public NxtSynchronizedMotors GetSynchronizedMotors(NxtMotorPort firstPort, NxtMotorPort secondPort) =>
        new(GetMotor(firstPort), GetMotor(secondPort));
}
