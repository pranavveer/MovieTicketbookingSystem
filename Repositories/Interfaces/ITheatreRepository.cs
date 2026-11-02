using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface ITheatreRepository
{
    Task<IEnumerable<Theatre>> GetAllAsync(int? pageNumber = null, int? pageSize = null);
    Task<int> GetCountAsync();
    Task<Theatre?> GetByIdAsync(int id);
    Task<Theatre> AddAsync(Theatre theatre);
    Task UpdateAsync(Theatre theatre);
    Task DeleteAsync(Theatre theatre);
    Task<bool> HasScreensAsync(int theatreId);
}
