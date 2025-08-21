namespace DeviceAPI.Manager.Data.Entities;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }

    public Device(int id, string name, string status)
    {
        Id = id;
        Name = name;
        Status = status;
    }
}