using System.IO.Ports;
using NxtRemote;
using NxtRemote.Motors;

Console.WriteLine("--- Available Serial Ports ---");
foreach (var portName in SerialPort.GetPortNames()) Console.WriteLine(portName);
Console.WriteLine("------------------------------");

using var communication = new NxtBluetoothCommunication("COM4");
var controller = new NxtController(communication);

controller.PlayTone(450, 500);

var motors = controller.GetSynchronizedMotors(NxtMotorPort.B, NxtMotorPort.C);

await new AsyncStateMachine<State>()
    .WithStateTransition(State.Forward, () => motors.RunAsync(0.5f, 0.0f, 3600), State.Turning)
    .WithStateTransition(State.Turning, () => motors.RunAsync(0.5f, 1.0f, 3600), State.Forward)
    .RunAsync(State.Forward, TimeSpan.FromSeconds(10));

motors.Coast();

enum State
{
    Forward,
    Turning
}