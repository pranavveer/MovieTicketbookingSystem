using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync(int? showId = null, string? status = null);
    Task<Booking?> GetByIdAsync(int id);
    Task<IEnumerable<Booking>> GetByShowIdAsync(int showId);
    Task<Booking> CreateBookingTransactionAsync(Booking booking, IEnumerable<int> seatIds);
    Task CancelBookingTransactionAsync(Booking booking);
}
