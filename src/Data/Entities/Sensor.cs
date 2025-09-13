namespace DeviceAPI.Manager.Data.Entities;

public class Sensor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public double Value { get; set; }
    public int DeviceId { get; set; }
    public string Status { get; set; }

    public Sensor(int id, string name, string type, double value, int deviceId, string status = "Active")
    {
        Id = id;
        Name = name;
        Type = type;
        Value = value;
        DeviceId = deviceId;
        Status = status;
    }
}