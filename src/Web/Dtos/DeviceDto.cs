using System.Collections.Generic;
using System.Linq;

namespace DeviceAPI.Manager.Web.Dtos;

public class DeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public IEnumerable<SensorDto> Sensors { get; set; }

    public DeviceDto(int id, string name, string status, IEnumerable<SensorDto> sensors)
    {
        Id = id;
        Name = name;
        Status = status;
        Sensors = sensors;
    }
}