using MovieTicketBooking.DTOs.Shows;

namespace MovieTicketBooking.Services.Interfaces;

public interface IShowService
{
    Task<IEnumerable<ShowResponse>> GetAllAsync(DateTime? date = null, int? movieId = null, int? theatreId = null);
    Task<ShowResponse> GetByIdAsync(int id);
    Task<IEnumerable<ShowResponse>> GetByMovieIdAsync(int movieId);
    Task<IEnumerable<ShowResponse>> GetByTheatreIdAsync(int theatreId);
    Task<IEnumerable<ShowResponse>> SearchByDateAsync(DateOnly date);
    Task<ShowResponse> CreateAsync(CreateShowRequest request);
    Task<ShowResponse> UpdateAsync(int id, UpdateShowRequest request);
    Task DeleteAsync(int id);
    Task<IEnumerable<ShowSeatResponse>> GetShowSeatsAsync(int showId, string? statusFilter = null);
}
