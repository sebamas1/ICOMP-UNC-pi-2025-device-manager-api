using DeviceAPI.Manager.Data.Entities;

namespace DeviceAPI.Manager.Data.Interfaces;

public interface IDeviceRepository
{
    IEnumerable<Device> GetAll();
    Device? GetById(int id);
    IEnumerable<Sensor> GetSensors(int deviceId);
    Sensor? GetSensor(int deviceId, int sensorId);
    void Add(Device device);
    void Update(Device device);
    void Delete(Device device);
    void AddSensor(Sensor sensor);
    void UpdateSensor(Sensor sensor);
    void DeleteSensor(Sensor sensor);
}
