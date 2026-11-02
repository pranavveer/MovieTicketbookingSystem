using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class TheatreRepository : ITheatreRepository
{
    private readonly ApplicationDbContext _context;
    public TheatreRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Theatre>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
    {
        var query = _context.Theatres.AsNoTracking().Include(t => t.Screens).OrderBy(t => t.Name).AsQueryable();
        if (pageNumber > 0 && pageSize > 0)
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
        return await query.ToListAsync();
    }

    public async Task<int> GetCountAsync() => await _context.Theatres.CountAsync();

    public async Task<Theatre?> GetByIdAsync(int id) =>
        await _context.Theatres.Include(t => t.Screens).FirstOrDefaultAsync(t => t.TheatreId == id);

    public async Task<Theatre> AddAsync(Theatre theatre)
    {
        await _context.Theatres.AddAsync(theatre);
        await _context.SaveChangesAsync();
        return theatre;
    }

    public async Task UpdateAsync(Theatre theatre) { _context.Theatres.Update(theatre); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(Theatre theatre) { _context.Theatres.Remove(theatre); await _context.SaveChangesAsync(); }
    public async Task<bool> HasScreensAsync(int theatreId) => await _context.Screens.AnyAsync(s => s.TheatreId == theatreId);
}
