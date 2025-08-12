using DeviceAPI.Manager.Data.Entities;
using DeviceAPI.Manager.Data.Interfaces;

namespace DeviceAPI.Manager.Data.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private static readonly List<Device> _devices =
    [
        new Device(1, "Raspberry Pi", "online"),
        new Device(2, "ESP32", "offline"),
        new Device(3, "Arduino Mega", "online"),
    ];

    public IEnumerable<Device> GetAll() => _devices;
    public Device? GetById(int id) => _devices.FirstOrDefault(d => d.Id == id);
}
