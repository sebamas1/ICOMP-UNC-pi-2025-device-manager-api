namespace DeviceAPI.Manager.Web.Dtos;

public class SensorDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public double Value { get; set; }
    public string Status { get; set; }

    public SensorDto(int id, string name, string type, double value, string status)
    {
        Id = id;
        Name = name;
        Type = type;
        Value = value;
        Status = status;
    }
}