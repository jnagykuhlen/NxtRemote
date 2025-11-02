namespace NxtRemote.Sensors;

public class NxtUltrasonicSensor
{
    public const byte MaxDistance = byte.MaxValue;

    public IPollable<byte> Pollable { get; }
    private NxtSensorCommunication Communication { get; }

    public NxtUltrasonicSensor(NxtSensorCommunication communication, TimeSpan pollingInterval)
    {
        communication.SetInputModeAsync(NxtSensorType.LowSpeed9V, NxtSensorMode.Raw);
        Communication = communication;
        Pollable = new Polling<byte>(() => communication.DigitalReadByteAsync(0x42), pollingInterval);
    }

    public Task<byte> GetDistanceAsync() => Pollable.NextAsync(distance => distance);

    public Task<byte> WaitForDistanceAsync(Func<byte, bool> predicate) =>
        Pollable.WhenAsync(distance => distance, predicate);

    public Task<string> ReadMeasurementUnitsAsync() => Communication.DigitalReadStringAsync(0x14, 7);

    public async Task<string> ReadVersionAsync() =>
        (await Communication.DigitalReadStringAsync(0x00, 8)).TrimEnd('?');

    public async Task<string> ReadManufacturerAsync() =>
        (await Communication.DigitalReadStringAsync(0x08, 8)).TrimEnd('?');

    public async Task<string> ReadSensorTypeAsync() =>
        (await Communication.DigitalReadStringAsync(0x10, 8)).TrimEnd('?');
}