namespace NxtRemote.Sensors;

public class NxtUltrasonicSensor
{
    public const byte MaxDistance = byte.MaxValue;
    
    public IPollable<byte> Pollable { get; }
    private NxtSensorCommunication Communication { get; }

    public NxtUltrasonicSensor(NxtSensorCommunication communication, TimeSpan pollingInterval)
    {
        communication.SetInputMode(NxtSensorType.LowSpeed9V, NxtSensorMode.Raw);
        Communication = communication;
        Pollable = new Polling<byte>(() => communication.DigitalReadByte(0x42), pollingInterval);
    }
    
    public Task<byte> GetDistanceAsync() => Pollable.NextAsync(distance => distance);
    
    public Task<byte> WaitForDistanceAsync(Func<byte, bool> predicate) =>
        Pollable.WhenAsync(distance => distance, predicate);

    public string ReadMeasurementUnits() => Communication.DigitalReadString(0x14, 7);
    public string ReadVersion() => Communication.DigitalReadString(0x00, 8).TrimEnd('?');
    public string ReadManufacturer() => Communication.DigitalReadString(0x08, 8).TrimEnd('?');
    public string ReadSensorType() => Communication.DigitalReadString(0x10, 8).TrimEnd('?');
}