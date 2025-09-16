namespace DeviceAPI.Manager.Web.Dtos;

public class SensorReadingDto
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }

    public string Source { get; set; } // e.g., "sensor", "calculated", etc.

    public SensorReadingDto(DateTime timestamp, double value, string source)
    {
        Timestamp = timestamp;
        Value = value;
        Source = source;
    }
}