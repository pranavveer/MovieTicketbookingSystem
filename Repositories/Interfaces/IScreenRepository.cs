using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IScreenRepository
{
    Task<IEnumerable<Screen>> GetAllAsync(int? theatreId = null);
    Task<Screen?> GetByIdAsync(int id);
    Task<IEnumerable<Screen>> GetByTheatreIdAsync(int theatreId);
    Task<Screen> AddAsync(Screen screen);
    Task UpdateAsync(Screen screen);
    Task DeleteAsync(Screen screen);
    Task<bool> ExistsByNameAsync(int theatreId, string screenName, int? excludeScreenId = null);
    Task<bool> HasSeatsOrShowsAsync(int screenId);
}
