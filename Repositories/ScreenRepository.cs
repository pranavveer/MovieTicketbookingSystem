using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class ScreenRepository : IScreenRepository
{
    private readonly ApplicationDbContext _context;
    public ScreenRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Screen>> GetAllAsync(int? theatreId = null)
    {
        var query = _context.Screens.AsNoTracking().Include(s => s.Theatre).Include(s => s.Seats).AsQueryable();
        if (theatreId.HasValue) query = query.Where(s => s.TheatreId == theatreId.Value);
        return await query.OrderBy(s => s.ScreenName).ToListAsync();
    }

    public async Task<Screen?> GetByIdAsync(int id) =>
        await _context.Screens.Include(s => s.Theatre).Include(s => s.Seats).FirstOrDefaultAsync(s => s.ScreenId == id);

    public async Task<IEnumerable<Screen>> GetByTheatreIdAsync(int theatreId) =>
        await _context.Screens.AsNoTracking().Include(s => s.Theatre).Include(s => s.Seats)
            .Where(s => s.TheatreId == theatreId).OrderBy(s => s.ScreenName).ToListAsync();

    public async Task<Screen> AddAsync(Screen screen)
    {
        await _context.Screens.AddAsync(screen);
        await _context.SaveChangesAsync();
        return screen;
    }

    public async Task UpdateAsync(Screen screen) { _context.Screens.Update(screen); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(Screen screen) { _context.Screens.Remove(screen); await _context.SaveChangesAsync(); }

    public async Task<bool> ExistsByNameAsync(int theatreId, string screenName, int? excludeScreenId = null) =>
        await _context.Screens.AnyAsync(s => s.TheatreId == theatreId && s.ScreenName == screenName &&
                                             (!excludeScreenId.HasValue || s.ScreenId != excludeScreenId.Value));

    public async Task<bool> HasSeatsOrShowsAsync(int screenId) =>
        await _context.Seats.AnyAsync(s => s.ScreenId == screenId) ||
        await _context.Shows.AnyAsync(s => s.ScreenId == screenId);
}
