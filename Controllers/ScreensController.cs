using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreensController : ControllerBase
{
    private readonly IScreenService _svc;
    public ScreensController(IScreenService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScreenResponse>>> GetAll([FromQuery] int? theatreId) => Ok(await _svc.GetAllAsync(theatreId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScreenResponse>> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpGet("theatre/{theatreId:int}")]
    public async Task<ActionResult<IEnumerable<ScreenResponse>>> GetByTheatre(int theatreId) => Ok(await _svc.GetByTheatreIdAsync(theatreId));

    [HttpPost]
    public async Task<ActionResult<ScreenResponse>> Create([FromBody] CreateScreenRequest req)
    {
        var created = await _svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.ScreenId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScreenResponse>> Update(int id, [FromBody] UpdateScreenRequest req) => Ok(await _svc.UpdateAsync(id, req));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) { await _svc.DeleteAsync(id); return NoContent(); }
}
