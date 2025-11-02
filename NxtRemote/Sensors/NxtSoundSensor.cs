namespace NxtRemote.Sensors;

public class NxtSoundSensor {
    public IPollable<NxtSensorInputValues> Pollable { get; }

    public NxtSoundSensor(NxtSensorCommunication communication, TimeSpan pollingInterval, bool humanWeighted) {
        communication.SetInputModeAsync(humanWeighted ? NxtSensorType.SoundDba : NxtSensorType.SoundDb, NxtSensorMode.PercentageFullScale);
        Pollable = new Polling<NxtSensorInputValues>(communication.GetInputValuesAsync, pollingInterval);
    }

    public Task<float> GetSoundLevelAsync() => Pollable.NextAsync(GetSoundLevel);

    public Task<float> WaitForSoundLevelAsync(Func<float, bool> predicate) =>
        Pollable.WhenAsync(GetSoundLevel, predicate);

    protected static float GetSoundLevel(NxtSensorInputValues inputValues) {
        if (!inputValues.Valid)
            throw new InvalidOperationException("Sensor input values are not valid.");

        return 0.001f * inputValues.NormalizedValue;
    }
}