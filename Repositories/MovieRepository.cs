using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly ApplicationDbContext _context;
    public MovieRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Movie>> GetAllAsync(int? pageNumber = null, int? pageSize = null, string? title = null)
    {
        var query = _context.Movies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(m => m.Title.Contains(title));
        query = query.OrderByDescending(m => m.ReleaseDate);
        if (pageNumber > 0 && pageSize > 0)
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
        return await query.ToListAsync();
    }

    public async Task<int> GetCountAsync(string? title = null)
    {
        var query = _context.Movies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(m => m.Title.Contains(title));
        return await query.CountAsync();
    }

    public async Task<Movie?> GetByIdAsync(int id) =>
        await _context.Movies.Include(m => m.Shows).FirstOrDefaultAsync(m => m.MovieId == id);

    public async Task<IEnumerable<Movie>> SearchByTitleAsync(string title) =>
        await _context.Movies.AsNoTracking().Where(m => m.Title.Contains(title)).OrderBy(m => m.Title).ToListAsync();

    public async Task<Movie> AddAsync(Movie movie)
    {
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();
        return movie;
    }

    public async Task UpdateAsync(Movie movie) { _context.Movies.Update(movie); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(Movie movie) { _context.Movies.Remove(movie); await _context.SaveChangesAsync(); }
    public async Task<bool> HasShowsAsync(int movieId) => await _context.Shows.AnyAsync(s => s.MovieId == movieId);
}
