using MovieTicketBooking.DTOs.Seats;

namespace MovieTicketBooking.Services.Interfaces;

public interface ISeatService
{
    Task<IEnumerable<SeatResponse>> GetByScreenIdAsync(int screenId);
    Task<SeatResponse> GetByIdAsync(int id);
    Task<SeatResponse> CreateAsync(CreateSeatRequest request);
    Task<IEnumerable<SeatResponse>> BulkCreateAsync(BulkCreateSeatsRequest request);
    Task<SeatResponse> UpdateAsync(int id, UpdateSeatRequest request);
    Task DeleteAsync(int id);
}
