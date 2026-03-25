using DeviceAPI.Manager.Data.Entities;
using DeviceAPI.Manager.Data.Repositories;
using Xunit;

namespace DeviceAPI.Manager.Web.Tests;

public class DeviceRepositoryTests
{
    [Fact]
    public void GetAll_Returns_SeededDevices()
    {
        var repo = new DeviceRepository();

        var devices = repo.GetAll().ToList();

        Assert.NotEmpty(devices);
        Assert.Contains(devices, d => d.Id == 1);
    }

    [Fact]
    public void GetById_Returns_Device_WhenExists()
    {
        var repo = new DeviceRepository();

        var device = repo.GetById(1);

        Assert.NotNull(device);
        Assert.Equal(1, device!.Id);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenNotExists()
    {
        var repo = new DeviceRepository();

        var device = repo.GetById(999);

        Assert.Null(device);
    }

    [Fact]
    public void GetSensors_ReturnsSensorsForDevice()
    {
        var repo = new DeviceRepository();

        var sensors = repo.GetSensors(1).ToList();

        Assert.NotEmpty(sensors);
        Assert.All(sensors, s => Assert.Equal(1, s.DeviceId));
    }

    [Fact]
    public void GetSensor_ReturnsSensor_WhenExists()
    {
        var repo = new DeviceRepository();

        var sensor = repo.GetSensor(1, 1);

        Assert.NotNull(sensor);
        Assert.Equal(1, sensor!.Id);
    }

    [Fact]
    public void GetSensor_ReturnsNull_WhenNotExists()
    {
        var repo = new DeviceRepository();

        var sensor = repo.GetSensor(1, 999);

        Assert.Null(sensor);
    }

    [Fact]
    public void Add_AppendNewDevice()
    {
        var repo = new DeviceRepository();
        var device = new Device(100, "Test", "online");

        repo.Add(device);

        var retrieved = repo.GetById(100);
        Assert.NotNull(retrieved);
        Assert.Equal("Test", retrieved!.Name);
    }

    [Fact]
    public void Update_ModifiesExistingDevice()
    {
        var repo = new DeviceRepository();
        var existing = repo.GetAll().First();
        var updated = new Device(existing.Id, "Updated", "offline");

        repo.Update(updated);

        var retrieved = repo.GetById(existing.Id);
        Assert.Equal("Updated", retrieved!.Name);
        Assert.Equal("offline", retrieved.Status);
    }

    [Fact]
    public void Delete_RemovesDevice()
    {
        var repo = new DeviceRepository();
        var device = new Device(200, "ToDelete", "offline");
        repo.Add(device);

        repo.Delete(device);

        Assert.Null(repo.GetById(200));
    }

    [Fact]
    public void AddSensor_AppendsSensor()
    {
        var repo = new DeviceRepository();
        var sensor = new Sensor(500, "X", "Temp", 10.0, 1);

        repo.AddSensor(sensor);

        var retrieved = repo.GetSensor(1, 500);
        Assert.NotNull(retrieved);
        Assert.Equal("X", retrieved!.Name);
    }

    [Fact]
    public void UpdateSensor_ModifiesExistingSensor()
    {
        var repo = new DeviceRepository();
        var existing = repo.GetSensors(1).First();
        var updated = new Sensor(existing.Id, existing.Name, "UpdatedType", 99.9, existing.DeviceId);

        repo.UpdateSensor(updated);

        var retrieved = repo.GetSensor(existing.DeviceId, existing.Id);
        Assert.Equal("UpdatedType", retrieved!.Type);
        Assert.Equal(99.9, retrieved.Value);
    }

    [Fact]
    public void DeleteSensor_RemovesSensor()
    {
        var repo = new DeviceRepository();
        var sensor = new Sensor(600, "Temp", "T", 1.0, 1);
        repo.AddSensor(sensor);

        repo.DeleteSensor(sensor);

        Assert.Null(repo.GetSensor(1, 600));
    }
}

