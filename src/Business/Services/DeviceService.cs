using DeviceAPI.Manager.Business.Interfaces;
using DeviceAPI.Manager.Data.Entities;
using DeviceAPI.Manager.Data.Interfaces;

namespace DeviceAPI.Manager.Business.Services;

public class DeviceService(IDeviceRepository repo) : IDeviceService
{
    private readonly IDeviceRepository _repo = repo;

    public IEnumerable<Device> GetAll() => _repo.GetAll();
    public Device? GetById(int id) => _repo.GetById(id);
    public IEnumerable<Sensor> GetSensors(int deviceId) => _repo.GetSensors(deviceId);
    public Sensor? GetSensor(int deviceId, int sensorId) => _repo.GetSensor(deviceId, sensorId);
    public Device Create(Device device)
    {
        _repo.Add(device);
        return device;
    }

    public void Update(int id, Device device)
    {
        var existing = _repo.GetById(id);
        if (existing is null) throw new KeyNotFoundException($"Device with ID {id} not found.");

        existing.Name = device.Name;
        existing.Status = device.Status;
        _repo.Update(existing);
    }

    public void Delete(int id)
    {
        var device = _repo.GetById(id);
        if (device is null) throw new KeyNotFoundException($"Device with ID {id} not found.");
        _repo.Delete(device);
    }

    public Sensor AddSensor(int deviceId, Sensor sensor)
    {
        var device = _repo.GetById(deviceId);
        if (device is null) throw new KeyNotFoundException($"Device with ID {deviceId} not found.");

        sensor.DeviceId = deviceId;
        _repo.AddSensor(sensor);
        return sensor;
    }

    public void UpdateSensor(int deviceId, int sensorId, Sensor sensor)
    {
        var existing = _repo.GetSensor(deviceId, sensorId);
        if (existing is null) throw new KeyNotFoundException($"Sensor with ID {sensorId} not found for device {deviceId}.");

        existing.Type = sensor.Type;
        existing.Value = sensor.Value;
        _repo.UpdateSensor(existing);
    }

    public void DeleteSensor(int deviceId, int sensorId)
    {
        var sensor = _repo.GetSensor(deviceId, sensorId);
        if (sensor is null) throw new KeyNotFoundException($"Sensor with ID {sensorId} not found for device {deviceId}.");
        _repo.DeleteSensor(sensor);
    }

    public IEnumerable<SensorReading> GetSensorHistory(int deviceId, int sensorId, int limit = 50)
    {
        // Simulate historical data for demonstration purposes
        var random = new Random();
        var readings = new List<SensorReading>();
        var current = DateTime.UtcNow.AddDays(-1); // Start from 1 day ago
        
        var value = random.NextDouble() * 100; // Random value between 0 and 100
        readings.Add(new SensorReading(current, value, "simulated"));
        current = current.AddHours(1); // Increment by 1 hour
        

        return readings;
    }
}
