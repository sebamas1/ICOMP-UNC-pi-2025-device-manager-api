using DeviceAPI.Manager.Data.Entities;

namespace DeviceAPI.Manager.Data.Interfaces;

public interface IDeviceRepository
{
    IEnumerable<Device> GetAll();
    Device? GetById(int id);
}
