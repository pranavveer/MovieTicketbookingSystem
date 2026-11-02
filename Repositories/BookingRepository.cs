using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;
    public BookingRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Booking>> GetAllAsync(int? showId = null, string? status = null)
    {
        var query = _context.Bookings.AsNoTracking()
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat).AsQueryable();
        if (showId.HasValue) query = query.Where(b => b.ShowId == showId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(b => b.Status.ToLower() == status.ToLower());
        return await query.OrderByDescending(b => b.BookingDate).ToListAsync();
    }

    public async Task<Booking?> GetByIdAsync(int id) =>
        await _context.Bookings
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
            .FirstOrDefaultAsync(b => b.BookingId == id);

    public async Task<IEnumerable<Booking>> GetByShowIdAsync(int showId) =>
        await _context.Bookings.AsNoTracking()
            .Include(b => b.Show).ThenInclude(s => s.Movie)
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
            .Where(b => b.ShowId == showId).OrderByDescending(b => b.BookingDate).ToListAsync();

    public async Task<Booking> CreateBookingTransactionAsync(Booking booking, IEnumerable<int> seatIds)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            var seatIdList = seatIds.ToList();
            var showSeats = await _context.ShowSeats
                .Where(ss => ss.ShowId == booking.ShowId && seatIdList.Contains(ss.SeatId))
                .ToListAsync();

            foreach (var ss in showSeats) ss.Status = "Booked";
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return booking;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task CancelBookingTransactionAsync(Booking booking)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            booking.Status = "Cancelled";
            _context.Bookings.Update(booking);

            var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
            var showSeats = await _context.ShowSeats
                .Where(ss => ss.ShowId == booking.ShowId && seatIds.Contains(ss.SeatId))
                .ToListAsync();

            foreach (var ss in showSeats) ss.Status = "Available";
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch { await tx.RollbackAsync(); throw; }
    }
}
