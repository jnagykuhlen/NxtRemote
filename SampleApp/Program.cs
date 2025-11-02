using System.IO.Ports;
using NxtRemote;
using NxtRemote.Motors;
using NxtRemote.Sensors;

Console.WriteLine("--- Available Serial Ports ---");
foreach (var portName in SerialPort.GetPortNames()) Console.WriteLine(portName);
Console.WriteLine("------------------------------");

using var communication = new NxtBluetoothCommunication("COM4");
var controller = new NxtController(communication);

controller.PlayTone(450, 500);

var ultrasonicSensor = controller.GetUltrasonicSensor(NxtSensorPort.Port1);
var motors = controller.GetSynchronizedMotors(NxtMotorPort.B, NxtMotorPort.C);

await new AsyncStateMachine<State>()
    .WithStateTransition(State.Forward, async () =>
    {
        motors.Run(-0.5f, 0.0f);
        await ultrasonicSensor.WaitForDistanceAsync(distance => distance < 30);
        motors.Coast();
        return State.Turning;
    })
    .WithStateTransition(State.Turning, async () =>
    {
        await motors.RunAsync(-0.5f, 1.0f, Random.Shared.Next(500, 2000));
        motors.ResetPosition(NxtResetMotorMode.Relative);
        return State.Forward;
    })
    .RunAsync(State.Forward, TimeSpan.FromSeconds(30));

motors.Coast();

enum State
{
    Forward,
    Turning
}
