using DeviceAPI.Manager.Data.Entities;

namespace DeviceAPI.Manager.Business.Interfaces;

public interface IDeviceService
{
    IEnumerable<Device> GetAll();
    Device? GetById(int id);
    IEnumerable<Sensor> GetSensors(int deviceId);
    Sensor? GetSensor(int deviceId, int sensorId);
    Device Create(Device device);
    void Update(int id, Device device);
    void Delete(int id);
    Sensor AddSensor(int deviceId, Sensor sensor);
    void UpdateSensor(int deviceId, int sensorId, Sensor sensor);
    void DeleteSensor(int deviceId, int sensorId);
}