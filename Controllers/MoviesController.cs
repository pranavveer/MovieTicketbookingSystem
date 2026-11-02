using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Common;
using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _svc;
    public MoviesController(IMovieService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MovieResponse>>> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? title)
        => Ok(await _svc.GetAllAsync(pageNumber, pageSize, title));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieResponse>> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<MovieResponse>>> Search([FromQuery] string title)
        => Ok(await _svc.SearchByTitleAsync(title));

    [HttpPost]
    public async Task<ActionResult<MovieResponse>> Create([FromBody] CreateMovieRequest req)
    {
        var created = await _svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.MovieId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieResponse>> Update(int id, [FromBody] UpdateMovieRequest req)
        => Ok(await _svc.UpdateAsync(id, req));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) { await _svc.DeleteAsync(id); return NoContent(); }
}
