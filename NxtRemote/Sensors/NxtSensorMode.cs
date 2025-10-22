namespace NxtRemote.Sensors;

public enum NxtSensorMode : byte
{
    Raw = 0x00,
    Boolean = 0x20,
    TransitionCount = 0x40,
    PeriodCount = 0x60,
    PercentageFullScale = 0x80,
    TemperatureCelsius = 0xA0,
    TemperatureFahrenheit = 0xC0,
    AngleStep = 0xE0 
}
