using MovieTicketBooking.DTOs.Common;
using MovieTicketBooking.DTOs.Theatres;

namespace MovieTicketBooking.Services.Interfaces;

public interface ITheatreService
{
    Task<PagedResult<TheatreResponse>> GetAllAsync(int? pageNumber = null, int? pageSize = null);
    Task<TheatreResponse> GetByIdAsync(int id);
    Task<TheatreResponse> CreateAsync(CreateTheatreRequest request);
    Task<TheatreResponse> UpdateAsync(int id, UpdateTheatreRequest request);
    Task DeleteAsync(int id);
}
