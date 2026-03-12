using DeviceAPI.Manager.Business.Interfaces;
using DeviceAPI.Manager.Data.Entities;
using DeviceAPI.Manager.Web.Controllers;
using DeviceAPI.Manager.Web.Dtos;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DeviceAPI.Manager.Web.Tests;

public class DevicesControllerTests
{
    private class FakeDeviceService : IDeviceService
    {
        public List<Device> Devices { get; } =
        [
            new Device(1, "D1", "online"),
            new Device(2, "D2", "offline")
        ];

        public List<Sensor> Sensors { get; } =
        [
            new Sensor(1, "S1", "T", 10, 1),
            new Sensor(2, "S2", "H", 20, 1),
            new Sensor(3, "S3", "T", 30, 2)
        ];

        public IEnumerable<Device> GetAll() => Devices;
        public Device? GetById(int id) => Devices.FirstOrDefault(d => d.Id == id);
        public IEnumerable<Sensor> GetSensors(int deviceId) => Sensors.Where(s => s.DeviceId == deviceId);
        public Sensor? GetSensor(int deviceId, int sensorId) =>
            Sensors.FirstOrDefault(s => s.DeviceId == deviceId && s.Id == sensorId);

        public Device Create(Device device)
        {
            Devices.Add(device);
            return device;
        }

        public void Update(int id, Device device)
        {
            var existing = GetById(id);
            if (existing is null) throw new KeyNotFoundException();
            existing.Name = device.Name;
            existing.Status = device.Status;
        }

        public void Delete(int id)
        {
            var device = GetById(id);
            if (device is null) throw new KeyNotFoundException();
            Devices.Remove(device);
        }

        public Sensor AddSensor(int deviceId, Sensor sensor)
        {
            sensor.DeviceId = deviceId;
            Sensors.Add(sensor);
            return sensor;
        }

        public void UpdateSensor(int deviceId, int sensorId, Sensor sensor)
        {
            var existing = GetSensor(deviceId, sensorId);
            if (existing is null) throw new KeyNotFoundException();
            existing.Name = sensor.Name;
            existing.Type = sensor.Type;
            existing.Value = sensor.Value;
        }

        public void DeleteSensor(int deviceId, int sensorId)
        {
            var sensor = GetSensor(deviceId, sensorId);
            if (sensor is null) throw new KeyNotFoundException();
            Sensors.Remove(sensor);
        }

        public IEnumerable<SensorReading> GetSensorHistory(int deviceId, int sensorId, int limit = 50)
        {
            return
            [
                new SensorReading(DateTime.UtcNow.AddMinutes(-1), 10, "simulated"),
                new SensorReading(DateTime.UtcNow, 11, "simulated")
            ];
        }
    }

    private static DevicesController CreateController(FakeDeviceService? service = null)
    {
        return new DevicesController(service ?? new FakeDeviceService());
    }

    [Fact]
    public void GetAll_ReturnsOkWithDevices()
    {
        var controller = CreateController();

        var result = controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsAssignableFrom<IEnumerable<DeviceDto>>(ok.Value);
        Assert.NotEmpty(value);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenDeviceMissing()
    {
        var controller = CreateController();

        var result = controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetById_ReturnsOk_WhenDeviceExists()
    {
        var controller = CreateController();

        var result = controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<DeviceDto>(ok.Value);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public void GetStatus_ReturnsStatus_WhenDeviceExists()
    {
        var controller = CreateController();

        var result = controller.GetStatus(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("online", ok.Value);
    }

    [Fact]
    public void GetSensors_ReturnsNotFound_WhenNoSensors()
    {
        var service = new FakeDeviceService();
        service.Sensors.Clear();
        var controller = CreateController(service);

        var result = controller.GetSensors(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetSensors_ReturnsOk_WhenSensorsExist()
    {
        var controller = CreateController();

        var result = controller.GetSensors(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<SensorDto>>(ok.Value);
        Assert.NotEmpty(dtos);
    }

    [Fact]
    public void GetSensor_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController();

        var result = controller.GetSensor(1, 999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetSensor_ReturnsOk_WhenExists()
    {
        var controller = CreateController();

        var result = controller.GetSensor(1, 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SensorDto>(ok.Value);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public void GetSensorStatus_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController();

        var result = controller.GetSensorStatus(1, 999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetSensorStatus_ReturnsOk_WhenExists()
    {
        var controller = CreateController();

        var result = controller.GetSensorStatus(1, 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void CreateDevice_ReturnsCreated()
    {
        var controller = CreateController();
        var dto = new DeviceDto(10, "New", "online", new List<SensorDto>());

        var result = controller.CreateDevice(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var value = Assert.IsType<DeviceDto>(created.Value);
        Assert.Equal(10, value.Id);
    }

    [Fact]
    public void CreateSensor_ReturnsCreated()
    {
        var controller = CreateController();
        var dto = new SensorDto(10, "S", "T", 1, "Active");

        var result = controller.CreateSensor(1, dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var value = Assert.IsType<SensorDto>(created.Value);
        Assert.Equal(10, value.Id);
    }

    [Fact]
    public void UpdateDevice_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController();
        var dto = new DeviceDto(999, "X", "offline", new List<SensorDto>());

        var result = controller.UpdateDevice(999, dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void UpdateDevice_ReturnsOk_WhenExists()
    {
        var controller = CreateController();
        var dto = new DeviceDto(1, "Updated", "offline", new List<SensorDto>());

        var result = controller.UpdateDevice(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<DeviceDto>(ok.Value);
        Assert.Equal("Updated", value.Name);
    }

    [Fact]
    public void UpdateSensor_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController();
        var dto = new SensorDto(999, "S", "T", 1, "Active");

        var result = controller.UpdateSensor(1, 999, dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void UpdateSensor_ReturnsOk_WhenExists()
    {
        var controller = CreateController();
        var dto = new SensorDto(1, "S1", "NEW", 99, "Active");

        var result = controller.UpdateSensor(1, 1, dto);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<SensorDto>(ok.Value);
        Assert.Equal("NEW", value.Type);
    }

    [Fact]
    public void DeleteDevice_InvokesService()
    {
        var service = new FakeDeviceService();
        var controller = CreateController(service);

        controller.DeleteDevice(1);

        Assert.DoesNotContain(service.Devices, d => d.Id == 1);
    }

    [Fact]
    public void DeleteSensor_InvokesService()
    {
        var service = new FakeDeviceService();
        var controller = CreateController(service);

        controller.DeleteSensor(1, 1);

        Assert.DoesNotContain(service.Sensors, s => s.Id == 1 && s.DeviceId == 1);
    }

    [Fact]
    public void GetHealth_ReturnsOk()
    {
        var controller = CreateController();

        var result = controller.GetHealth();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void GetSensorHistory_ReturnsNotFound_WhenSensorMissing()
    {
        var controller = CreateController();

        var result = controller.GetSensorHistory(1, 999, 10);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetSensorHistory_ReturnsOk_WithHistory()
    {
        var controller = CreateController();

        var result = controller.GetSensorHistory(1, 1, 10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var history = Assert.IsAssignableFrom<IEnumerable<SensorReading>>(ok.Value);
        Assert.NotEmpty(history);
    }
}

