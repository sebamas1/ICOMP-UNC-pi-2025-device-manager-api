namespace DeviceAPI.Manager.Data.Entities;

public class SensorReading
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string Source { get; set; } // e.g., "sensor", "calculated", etc.

    public SensorReading(DateTime timestamp, double value, string source)
    {
        Timestamp = timestamp;
        Value = value;
        Source = source;
    }
}