using DeviceAPI.Manager.Business.Services;
using DeviceAPI.Manager.Data.Entities;
using DeviceAPI.Manager.Data.Interfaces;
using Xunit;

namespace DeviceAPI.Manager.Web.Tests;

public class DeviceServiceTests
{
    private class FakeDeviceRepository : IDeviceRepository
    {
        public List<Device> Devices { get; } = new()
        {
            new Device(1, "D1", "online"),
            new Device(2, "D2", "offline")
        };

        public List<Sensor> Sensors { get; } = new()
        {
            new Sensor(1, "S1", "T", 10, 1),
            new Sensor(2, "S2", "H", 20, 1),
            new Sensor(3, "S3", "T", 30, 2)
        };

        public IEnumerable<Device> GetAll() => Devices;
        public Device? GetById(int id) => Devices.FirstOrDefault(d => d.Id == id);
        public IEnumerable<Sensor> GetSensors(int deviceId) => Sensors.Where(s => s.DeviceId == deviceId);
        public Sensor? GetSensor(int deviceId, int sensorId) =>
            Sensors.FirstOrDefault(s => s.DeviceId == deviceId && s.Id == sensorId);

        public void Add(Device device) => Devices.Add(device);

        public void Update(Device device)
        {
            var existing = GetById(device.Id);
            if (existing is null) return;
            existing.Name = device.Name;
            existing.Status = device.Status;
        }

        public void Delete(Device device) => Devices.Remove(device);

        public void AddSensor(Sensor sensor) => Sensors.Add(sensor);

        public void UpdateSensor(Sensor sensor)
        {
            var existing = GetSensor(sensor.DeviceId, sensor.Id);
            if (existing is null) return;
            existing.Type = sensor.Type;
            existing.Value = sensor.Value;
        }

        public void DeleteSensor(Sensor sensor) => Sensors.Remove(sensor);
    }

    [Fact]
    public void GetAll_DelegatesToRepository()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);

        var result = service.GetAll().ToList();

        Assert.Equal(repo.Devices.Count, result.Count);
    }

    [Fact]
    public void Create_AddsDevice()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);
        var device = new Device(10, "New", "online");

        var created = service.Create(device);

        Assert.Contains(repo.Devices, d => d.Id == created.Id);
    }

    [Fact]
    public void Update_Throws_WhenDeviceNotFound()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);
        var device = new Device(999, "X", "offline");

        Assert.Throws<KeyNotFoundException>(() => service.Update(999, device));
    }

    [Fact]
    public void Update_UpdatesExistingDevice()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);
        var device = new Device(1, "Updated", "offline");

        service.Update(1, device);

        var updated = repo.Devices.First(d => d.Id == 1);
        Assert.Equal("Updated", updated.Name);
        Assert.Equal("offline", updated.Status);
    }

    [Fact]
    public void Delete_Throws_WhenDeviceNotFound()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);

        Assert.Throws<KeyNotFoundException>(() => service.Delete(999));
    }

    [Fact]
    public void Delete_RemovesDevice()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);

        service.Delete(1);

        Assert.DoesNotContain(repo.Devices, d => d.Id == 1);
    }

    [Fact]
    public void AddSensor_Throws_WhenDeviceNotFound()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);
        var sensor = new Sensor(10, "S", "T", 1, 999);

        Assert.Throws<KeyNotFoundException>(() => service.AddSensor(999, sensor));
    }

    [Fact]
    public void AddSensor_AddsSensorToDevice()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);
        var sensor = new Sensor(10, "S", "T", 1, 0);

        var created = service.AddSensor(1, sensor);

        Assert.Equal(1, created.DeviceId);
        Assert.Contains(repo.Sensors, s => s.Id == created.Id && s.DeviceId == 1);
    }

    [Fact]
    public void UpdateSensor_Throws_WhenSensorNotFound()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);
        var sensor = new Sensor(999, "S", "T", 1, 1);

        Assert.Throws<KeyNotFoundException>(() => service.UpdateSensor(1, 999, sensor));
    }

    [Fact]
    public void UpdateSensor_UpdatesExistingSensor()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);
        var sensor = new Sensor(1, "S1", "NEW", 99, 1);

        service.UpdateSensor(1, 1, sensor);

        var updated = repo.Sensors.First(s => s.Id == 1);
        Assert.Equal("NEW", updated.Type);
        Assert.Equal(99, updated.Value);
    }

    [Fact]
    public void DeleteSensor_Throws_WhenSensorNotFound()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);

        Assert.Throws<KeyNotFoundException>(() => service.DeleteSensor(1, 999));
    }

    [Fact]
    public void DeleteSensor_RemovesSensor()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);

        service.DeleteSensor(1, 1);

        Assert.DoesNotContain(repo.Sensors, s => s.Id == 1 && s.DeviceId == 1);
    }

    [Fact]
    public void GetSensorHistory_ReturnsAtLeastOneReading()
    {
        var repo = new FakeDeviceRepository();
        var service = new DeviceService(repo);

        var history = service.GetSensorHistory(1, 1, 10).ToList();

        Assert.NotEmpty(history);
        var reading = history.First();
        Assert.InRange(reading.Value, 0, 100);
        Assert.False(string.IsNullOrWhiteSpace(reading.Source));
    }
}

