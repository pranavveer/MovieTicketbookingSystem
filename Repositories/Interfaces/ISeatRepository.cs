using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface ISeatRepository
{
    Task<IEnumerable<Seat>> GetByScreenIdAsync(int screenId);
    Task<Seat?> GetByIdAsync(int id);
    Task<Seat> AddAsync(Seat seat);
    Task AddRangeAsync(IEnumerable<Seat> seats);
    Task UpdateAsync(Seat seat);
    Task DeleteAsync(Seat seat);
    Task<bool> ExistsAsync(int screenId, string rowName, int seatNumber, int? excludeSeatId = null);
    Task<bool> HasShowSeatsOrBookingsAsync(int seatId);
}
