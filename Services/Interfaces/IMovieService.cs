using MovieTicketBooking.DTOs.Common;
using MovieTicketBooking.DTOs.Movies;

namespace MovieTicketBooking.Services.Interfaces;

public interface IMovieService
{
    Task<PagedResult<MovieResponse>> GetAllAsync(int? pageNumber = null, int? pageSize = null, string? title = null);
    Task<MovieResponse> GetByIdAsync(int id);
    Task<IEnumerable<MovieResponse>> SearchByTitleAsync(string title);
    Task<MovieResponse> CreateAsync(CreateMovieRequest request);
    Task<MovieResponse> UpdateAsync(int id, UpdateMovieRequest request);
    Task DeleteAsync(int id);
}
