using System.IO.Ports;
using NxtRemote;
using NxtRemote.Motors;

Console.WriteLine("--- Available Serial Ports ---");
foreach (var portName in SerialPort.GetPortNames()) Console.WriteLine(portName);
Console.WriteLine("------------------------------");

using var communication = new NxtBluetoothCommunication("COM4");
var controller = new NxtController(communication);
var motor = controller.GetMotor(NxtMotorPort.B);

await Task.WhenAny(motor.RunAsync(0.5f, 720), Task.Delay(5000));

motor.Coast();
