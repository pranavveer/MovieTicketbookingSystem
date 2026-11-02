using MovieTicketBooking.DTOs.Screens;

namespace MovieTicketBooking.Services.Interfaces;

public interface IScreenService
{
    Task<IEnumerable<ScreenResponse>> GetAllAsync(int? theatreId = null);
    Task<ScreenResponse> GetByIdAsync(int id);
    Task<IEnumerable<ScreenResponse>> GetByTheatreIdAsync(int theatreId);
    Task<ScreenResponse> CreateAsync(CreateScreenRequest request);
    Task<ScreenResponse> UpdateAsync(int id, UpdateScreenRequest request);
    Task DeleteAsync(int id);
}
