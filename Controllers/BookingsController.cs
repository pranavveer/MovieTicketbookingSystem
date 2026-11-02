using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _svc;
    public BookingsController(IBookingService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingResponse>>> GetAll([FromQuery] int? showId, [FromQuery] string? status)
        => Ok(await _svc.GetAllAsync(showId, status));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpGet("show/{showId:int}")]
    public async Task<ActionResult<IEnumerable<BookingResponse>>> GetByShow(int showId) => Ok(await _svc.GetByShowIdAsync(showId));

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingRequest req)
    {
        var created = await _svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.BookingId }, created);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<ActionResult<BookingResponse>> Cancel(int id) => Ok(await _svc.CancelAsync(id));
}
