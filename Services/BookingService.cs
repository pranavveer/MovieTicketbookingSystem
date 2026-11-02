using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IShowRepository _showRepo;

    public BookingService(IBookingRepository bookingRepo, IShowRepository showRepo)
    {
        _bookingRepo = bookingRepo; _showRepo = showRepo;
    }

    public async Task<IEnumerable<BookingResponse>> GetAllAsync(int? showId = null, string? status = null) =>
        (await _bookingRepo.GetAllAsync(showId, status)).Select(Map);

    public async Task<BookingResponse> GetByIdAsync(int id)
    {
        var b = await _bookingRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Booking with ID {id} not found.");
        return Map(b);
    }

    public async Task<IEnumerable<BookingResponse>> GetByShowIdAsync(int showId)
    {
        if (await _showRepo.GetByIdAsync(showId) == null) throw new NotFoundException($"Show with ID {showId} not found.");
        return (await _bookingRepo.GetByShowIdAsync(showId)).Select(Map);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest req)
    {
        if (req.SeatIds == null || !req.SeatIds.Any()) throw new BadRequestException("At least one seat must be selected.");
        var distinct = req.SeatIds.Distinct().ToList();
        if (distinct.Count != req.SeatIds.Count) throw new BadRequestException("Duplicate seat selections are not allowed.");

        var show = await _showRepo.GetByIdAsync(req.ShowId) ?? throw new NotFoundException($"Show with ID {req.ShowId} not found.");
        var showSeats = (await _showRepo.GetShowSeatsAsync(req.ShowId)).ToDictionary(ss => ss.SeatId);

        var invalid = distinct.Where(id => !showSeats.ContainsKey(id)).ToList();
        if (invalid.Any()) throw new BadRequestException($"Seat IDs [{string.Join(", ", invalid)}] do not belong to this show.");

        var unavailable = distinct.Where(id => !showSeats[id].Status.Equals("Available", StringComparison.OrdinalIgnoreCase))
            .Select(id => { var s = showSeats[id].Seat; return s != null ? $"{s.RowName}{s.SeatNumber}" : $"#{id}"; }).ToList();
        if (unavailable.Any()) throw new ConflictException($"Seat(s) [{string.Join(", ", unavailable)}] are no longer available.");

        var booking = new Booking
        {
            BookingReference = $"MOV{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}",
            ShowId = req.ShowId, CustomerName = req.CustomerName.Trim(),
            CustomerEmail = req.CustomerEmail.Trim(), CustomerPhone = req.CustomerPhone.Trim(),
            BookingDate = DateTime.UtcNow, TotalAmount = distinct.Count * show.TicketPrice, Status = "Confirmed"
        };
        foreach (var seatId in distinct)
            booking.BookingSeats.Add(new BookingSeat { SeatId = seatId, Price = show.TicketPrice });

        await _bookingRepo.CreateBookingTransactionAsync(booking, distinct);
        var created = await _bookingRepo.GetByIdAsync(booking.BookingId);
        return Map(created ?? booking);
    }

    public async Task<BookingResponse> CancelAsync(int id)
    {
        var b = await _bookingRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Booking with ID {id} not found.");
        if (!b.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            throw new ConflictException("Only confirmed bookings can be cancelled.");
        await _bookingRepo.CancelBookingTransactionAsync(b);
        return Map((await _bookingRepo.GetByIdAsync(id)) ?? b);
    }

    private static BookingResponse Map(Booking b) => new()
    {
        BookingId = b.BookingId, BookingReference = b.BookingReference, ShowId = b.ShowId,
        MovieTitle = b.Show?.Movie?.Title ?? string.Empty,
        CustomerName = b.CustomerName, CustomerEmail = b.CustomerEmail, CustomerPhone = b.CustomerPhone,
        BookingDate = b.BookingDate, TotalAmount = b.TotalAmount, Status = b.Status,
        Seats = b.BookingSeats.Select(bs => bs.Seat != null ? $"{bs.Seat.RowName}{bs.Seat.SeatNumber}" : $"#{bs.SeatId}")
                              .OrderBy(s => s).ToList()
    };
}
