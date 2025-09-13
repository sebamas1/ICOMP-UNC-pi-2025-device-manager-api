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

    private static readonly List<Sensor> _sensors =
    [
        new Sensor(1, "T1", "Temperature", 22.5, 1),
        new Sensor(2, "H1", "Humidity", 45.0, 1),
        new Sensor(3, "P1", "Pressure", 1013.25, 2),
        new Sensor(4, "P2", "Light", 300.0, 3),
    ];

    public IEnumerable<Device> GetAll() => _devices;
    public Device? GetById(int id) => _devices.FirstOrDefault(d => d.Id == id);
    public IEnumerable<Sensor> GetSensors(int deviceId) => _sensors.Where(s => s.DeviceId == deviceId);
    public Sensor? GetSensor(int deviceId, int sensorId) =>
        _sensors.FirstOrDefault(s => s.DeviceId == deviceId && s.Id == sensorId);
    public void Add(Device device) => _devices.Add(device);
    public void Update(Device device)
    {
        var existing = GetById(device.Id);
        if (existing is not null)
        {
            existing.Name = device.Name;
            existing.Status = device.Status;
        }
    }
    public void Delete(Device device) => _devices.Remove(device);
    public void AddSensor(Sensor sensor) => _sensors.Add(sensor);
    public void UpdateSensor(Sensor sensor)
    {
        var existing = GetSensor(sensor.DeviceId, sensor.Id);
        if (existing is not null)
        {
            existing.Type = sensor.Type;
            existing.Value = sensor.Value;
        }
    }
    public void DeleteSensor(Sensor sensor) => _sensors.Remove(sensor);
}
