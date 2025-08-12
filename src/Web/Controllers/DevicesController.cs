using DeviceAPI.Manager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DeviceAPI.Manager.Web.Dtos;

namespace DeviceAPI.Manager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceService service) : ControllerBase
{
    private readonly IDeviceService _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<DeviceDto>> GetAll()
    {
        return Ok();
        var items = _service.GetAll()
            .Select(d => new DeviceDto(d.Id, d.Name, d.Status));
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public ActionResult<DeviceDto> GetById(int id)
    {
        var d = _service.GetById(id);
        if (d is null) return NotFound();
        return Ok(new DeviceDto(d.Id, d.Name, d.Status));
    }
}
