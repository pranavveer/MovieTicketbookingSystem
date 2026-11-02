using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IShowRepository
{
    Task<IEnumerable<Show>> GetAllAsync(DateTime? date = null, int? movieId = null, int? theatreId = null);
    Task<Show?> GetByIdAsync(int id);
    Task<IEnumerable<Show>> GetByMovieIdAsync(int movieId);
    Task<IEnumerable<Show>> GetByTheatreIdAsync(int theatreId);
    Task<IEnumerable<Show>> SearchByDateAsync(DateOnly date);
    Task<Show> AddAsync(Show show);
    Task UpdateAsync(Show show);
    Task DeleteAsync(Show show);
    Task<bool> HasOverlappingShowAsync(int screenId, DateTime startTime, DateTime endTime, int? excludeShowId = null);
    Task<bool> HasBookingsAsync(int showId);
    Task<IEnumerable<ShowSeat>> GetShowSeatsAsync(int showId, string? statusFilter = null);
    Task AddShowSeatsAsync(IEnumerable<ShowSeat> showSeats);
}
