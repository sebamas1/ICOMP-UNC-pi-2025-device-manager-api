using DeviceAPI.Manager.Business.Interfaces;
using DeviceAPI.Manager.Data.Entities;
using DeviceAPI.Manager.Data.Interfaces;

namespace DeviceAPI.Manager.Business.Services;

public class DeviceService(IDeviceRepository repo) : IDeviceService
{
    private readonly IDeviceRepository _repo = repo;

    public IEnumerable<Device> GetAll() => _repo.GetAll();
    public Device? GetById(int id) => _repo.GetById(id);
}
