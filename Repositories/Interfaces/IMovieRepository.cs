using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IMovieRepository
{
    Task<IEnumerable<Movie>> GetAllAsync(int? pageNumber = null, int? pageSize = null, string? title = null);
    Task<int> GetCountAsync(string? title = null);
    Task<Movie?> GetByIdAsync(int id);
    Task<IEnumerable<Movie>> SearchByTitleAsync(string title);
    Task<Movie> AddAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(Movie movie);
    Task<bool> HasShowsAsync(int movieId);
}
