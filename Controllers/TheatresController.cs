using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Common;
using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TheatresController : ControllerBase
{
    private readonly ITheatreService _svc;
    public TheatresController(ITheatreService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<PagedResult<TheatreResponse>>> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        => Ok(await _svc.GetAllAsync(pageNumber, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TheatreResponse>> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<TheatreResponse>> Create([FromBody] CreateTheatreRequest req)
    {
        var created = await _svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.TheatreId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TheatreResponse>> Update(int id, [FromBody] UpdateTheatreRequest req)
        => Ok(await _svc.UpdateAsync(id, req));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) { await _svc.DeleteAsync(id); return NoContent(); }
}
