using DeviceAPI.Manager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DeviceAPI.Manager.Web.Dtos;
using DeviceAPI.Manager.Data.Entities;

namespace DeviceAPI.Manager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceService service) : ControllerBase
{
    private readonly IDeviceService _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<DeviceDto>> GetAll()
    {
        var items = _service.GetAll()
            .Select(d => new DeviceDto(d.Id, d.Name, d.Status, new List<SensorDto>()));
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public ActionResult<DeviceDto> GetById(int id)
    {
        var d = _service.GetById(id);
        if (d is null) return NotFound();
        return Ok(new DeviceDto(d.Id, d.Name, d.Status, new List<SensorDto>()));
    }

    [HttpGet("{id:int}/status")]
    public ActionResult<string> GetStatus(int id)
    {
        var device = _service.GetById(id);
        if (device is null) return NotFound();
        return Ok(device.Status); // ej: "Ok", "Offline", etc.
    }

    [HttpGet("{deviceId:int}/sensors")]
    public ActionResult<IEnumerable<SensorDto>> GetSensors(int deviceId)
    {
        var sensors = _service.GetSensors(deviceId);
        if (sensors is null || !sensors.Any()) return NotFound();
        return Ok(sensors.Select(s => new SensorDto(s.Id, s.Name, s.Type, s.Value, s.Status)));
    }

    [HttpGet("{deviceId:int}/sensors/{sensorId:int}")]
    public ActionResult<SensorDto> GetSensor(int deviceId, int sensorId)
    {
        var sensor = _service.GetSensor(deviceId, sensorId);
        if (sensor is null) return NotFound();
        return Ok(new SensorDto(sensor.Id, sensor.Name, sensor.Type, sensor.Value, sensor.Status));
    }

    [HttpGet("{deviceId:int}/sensors/{sensorId:int}/status")]
    public ActionResult<string> GetSensorStatus(int deviceId, int sensorId)
    {
        var sensor = _service.GetSensor(deviceId, sensorId);
        if (sensor is null) return NotFound();
        return Ok(sensor.Status);
    }

    [HttpPost]
    public ActionResult<DeviceDto> CreateDevice([FromBody] DeviceDto dto)
    {
        var device = new Device(dto.Id, dto.Name, dto.Status);
        if (device is null) return BadRequest("Invalid device data");
        var created = _service.Create(device);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new DeviceDto(created.Id, created.Name, created.Status, new List<SensorDto>()));
    }

    [HttpPost("{deviceId:int}/sensors")]
    public ActionResult<SensorDto> CreateSensor(int deviceId, [FromBody] SensorDto dto)
    {
        var sensor = new Sensor(dto.Id, dto.Name, dto.Type, dto.Value, deviceId);
        if (sensor is null) return BadRequest("Invalid sensor data");

        var created = _service.AddSensor(deviceId, sensor);
        if (created is null) return NotFound(); // Device inexistente
        return CreatedAtAction(nameof(GetSensor),
            new { deviceId, sensorId = created.Id },
            new SensorDto(created.Id, created.Name, created.Type, created.Value, created.Status));
    }

    [HttpPut("{id:int}")]
    public ActionResult<DeviceDto> UpdateDevice(int id, [FromBody] DeviceDto dto)
    {
        var existing = _service.GetById(id);
        if (existing is null) return NotFound($"Device with ID {id} not found.");

        existing.Name = dto.Name;
        existing.Status = dto.Status;

        _service.Update(id, existing);
        return Ok(new DeviceDto(existing.Id, existing.Name, existing.Status, new List<SensorDto>()));
    }

    [HttpPut("{deviceId:int}/sensors/{sensorId:int}")]
    public ActionResult<SensorDto> UpdateSensor(int deviceId, int sensorId, [FromBody] SensorDto dto)
    {
        var existing = _service.GetSensor(deviceId, sensorId);
        if (existing is null) return NotFound($"Sensor with ID {sensorId} not found for device {deviceId}.");

        existing.Name = dto.Name;
        existing.Type = dto.Type;
        existing.Value = dto.Value;

        _service.UpdateSensor(deviceId, sensorId, existing);
        return Ok(new SensorDto(existing.Id, existing.Name, existing.Type, existing.Value, existing.Status));
    }

    [HttpDelete("{id:int}")]
    public void DeleteDevice(int id)
    {
        _service.Delete(id);
    }

    [HttpDelete("{deviceId:int}/sensors/{sensorId:int}")]
    public void DeleteSensor(int deviceId, int sensorId)
    {
        _service.DeleteSensor(deviceId, sensorId);
    }

    [HttpGet("health")]
    public ActionResult GetHealth() => Ok(new { status = "healthy" });
    
    [HttpGet("{deviceId:int}/sensors/{sensorId:int}/history")]
    public ActionResult<IEnumerable<SensorReading>> GetSensorHistory(int deviceId, int sensorId, [FromQuery] int limit = 50)
    {
        var sensor = _service.GetSensor(deviceId, sensorId);
        if (sensor is null) return NotFound();

        var history = _service.GetSensorHistory(deviceId, sensorId, limit);
        return Ok(history);
    }

}