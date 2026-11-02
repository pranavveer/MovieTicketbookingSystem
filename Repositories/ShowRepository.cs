using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class ShowRepository : IShowRepository
{
    private readonly ApplicationDbContext _context;
    public ShowRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Show>> GetAllAsync(DateTime? date = null, int? movieId = null, int? theatreId = null)
    {
        var query = _context.Shows.AsNoTracking()
            .Include(s => s.Movie).Include(s => s.Screen).ThenInclude(sc => sc.Theatre)
            .Include(s => s.ShowSeats).AsQueryable();
        if (date.HasValue) query = query.Where(s => s.StartTime.Date == date.Value.Date);
        if (movieId.HasValue) query = query.Where(s => s.MovieId == movieId.Value);
        if (theatreId.HasValue) query = query.Where(s => s.Screen.TheatreId == theatreId.Value);
        return await query.OrderBy(s => s.StartTime).ToListAsync();
    }

    public async Task<Show?> GetByIdAsync(int id) =>
        await _context.Shows.Include(s => s.Movie).Include(s => s.Screen).ThenInclude(sc => sc.Theatre)
            .Include(s => s.ShowSeats).FirstOrDefaultAsync(s => s.ShowId == id);

    public async Task<IEnumerable<Show>> GetByMovieIdAsync(int movieId) =>
        await _context.Shows.AsNoTracking()
            .Include(s => s.Movie).Include(s => s.Screen).ThenInclude(sc => sc.Theatre)
            .Include(s => s.ShowSeats).Where(s => s.MovieId == movieId).OrderBy(s => s.StartTime).ToListAsync();

    public async Task<IEnumerable<Show>> GetByTheatreIdAsync(int theatreId) =>
        await _context.Shows.AsNoTracking()
            .Include(s => s.Movie).Include(s => s.Screen).ThenInclude(sc => sc.Theatre)
            .Include(s => s.ShowSeats).Where(s => s.Screen.TheatreId == theatreId).OrderBy(s => s.StartTime).ToListAsync();

    public async Task<IEnumerable<Show>> SearchByDateAsync(DateOnly date)
    {
        var d = date.ToDateTime(TimeOnly.MinValue);
        return await _context.Shows.AsNoTracking()
            .Include(s => s.Movie).Include(s => s.Screen).ThenInclude(sc => sc.Theatre)
            .Include(s => s.ShowSeats).Where(s => s.StartTime.Date == d.Date).OrderBy(s => s.StartTime).ToListAsync();
    }

    public async Task<Show> AddAsync(Show show)
    {
        await _context.Shows.AddAsync(show);
        await _context.SaveChangesAsync();
        return show;
    }

    public async Task UpdateAsync(Show show) { _context.Shows.Update(show); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(Show show) { _context.Shows.Remove(show); await _context.SaveChangesAsync(); }

    public async Task<bool> HasOverlappingShowAsync(int screenId, DateTime startTime, DateTime endTime, int? excludeShowId = null) =>
        await _context.Shows.AnyAsync(s => s.ScreenId == screenId &&
                                           (!excludeShowId.HasValue || s.ShowId != excludeShowId.Value) &&
                                           s.StartTime < endTime && s.EndTime > startTime);

    public async Task<bool> HasBookingsAsync(int showId) =>
        await _context.Bookings.AnyAsync(b => b.ShowId == showId);

    public async Task<IEnumerable<ShowSeat>> GetShowSeatsAsync(int showId, string? statusFilter = null)
    {
        var query = _context.ShowSeats.AsNoTracking().Include(ss => ss.Seat).Where(ss => ss.ShowId == showId);
        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(ss => ss.Status.ToLower() == statusFilter.ToLower());
        return await query.OrderBy(ss => ss.Seat.RowName).ThenBy(ss => ss.Seat.SeatNumber).ToListAsync();
    }

    public async Task AddShowSeatsAsync(IEnumerable<ShowSeat> showSeats)
    {
        await _context.ShowSeats.AddRangeAsync(showSeats);
        await _context.SaveChangesAsync();
    }
}
