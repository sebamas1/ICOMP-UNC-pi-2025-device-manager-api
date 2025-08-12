using DeviceAPI.Manager.Data.Entities;

namespace DeviceAPI.Manager.Business.Interfaces;

public interface IDeviceService
{
    IEnumerable<Device> GetAll();
    Device? GetById(int id);
}
