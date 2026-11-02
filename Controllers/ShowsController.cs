using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowsController : ControllerBase
{
    private readonly IShowService _svc;
    public ShowsController(IShowService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShowResponse>>> GetAll([FromQuery] DateTime? date, [FromQuery] int? movieId, [FromQuery] int? theatreId)
        => Ok(await _svc.GetAllAsync(date, movieId, theatreId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShowResponse>> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpGet("movie/{movieId:int}")]
    public async Task<ActionResult<IEnumerable<ShowResponse>>> GetByMovie(int movieId) => Ok(await _svc.GetByMovieIdAsync(movieId));

    [HttpGet("theatre/{theatreId:int}")]
    public async Task<ActionResult<IEnumerable<ShowResponse>>> GetByTheatre(int theatreId) => Ok(await _svc.GetByTheatreIdAsync(theatreId));

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ShowResponse>>> SearchByDate([FromQuery] DateOnly date) => Ok(await _svc.SearchByDateAsync(date));

    [HttpPost]
    public async Task<ActionResult<ShowResponse>> Create([FromBody] CreateShowRequest req)
    {
        var created = await _svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.ShowId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ShowResponse>> Update(int id, [FromBody] UpdateShowRequest req) => Ok(await _svc.UpdateAsync(id, req));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) { await _svc.DeleteAsync(id); return NoContent(); }

    [HttpGet("{showId:int}/seats")]
    public async Task<ActionResult<IEnumerable<ShowSeatResponse>>> GetSeats(int showId) => Ok(await _svc.GetShowSeatsAsync(showId));

    [HttpGet("{showId:int}/seats/available")]
    public async Task<ActionResult<IEnumerable<ShowSeatResponse>>> GetAvailableSeats(int showId) => Ok(await _svc.GetShowSeatsAsync(showId, "Available"));

    [HttpGet("{showId:int}/seats/booked")]
    public async Task<ActionResult<IEnumerable<ShowSeatResponse>>> GetBookedSeats(int showId) => Ok(await _svc.GetShowSeatsAsync(showId, "Booked"));
}
