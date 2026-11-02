using MovieTicketBooking.DTOs.Common;
using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repo;
    public MovieService(IMovieRepository repo) => _repo = repo;

    public async Task<PagedResult<MovieResponse>> GetAllAsync(int? pageNumber = null, int? pageSize = null, string? title = null)
    {
        var movies = await _repo.GetAllAsync(pageNumber, pageSize, title);
        var total = await _repo.GetCountAsync(title);
        return new PagedResult<MovieResponse>
        {
            Items = movies.Select(Map),
            TotalCount = total,
            PageNumber = pageNumber ?? 1,
            PageSize = pageSize ?? total
        };
    }

    public async Task<MovieResponse> GetByIdAsync(int id)
    {
        var movie = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Movie with ID {id} not found.");
        return Map(movie);
    }

    public async Task<IEnumerable<MovieResponse>> SearchByTitleAsync(string title) =>
        string.IsNullOrWhiteSpace(title)
            ? Enumerable.Empty<MovieResponse>()
            : (await _repo.SearchByTitleAsync(title)).Select(Map);

    public async Task<MovieResponse> CreateAsync(CreateMovieRequest req)
    {
        var movie = new Movie
        {
            Title = req.Title.Trim(), Description = req.Description?.Trim(),
            DurationMinutes = req.DurationMinutes, ReleaseDate = req.ReleaseDate,
            Language = req.Language.Trim(), Genre = req.Genre.Trim()
        };
        return Map(await _repo.AddAsync(movie));
    }

    public async Task<MovieResponse> UpdateAsync(int id, UpdateMovieRequest req)
    {
        var movie = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Movie with ID {id} not found.");
        movie.Title = req.Title.Trim(); movie.Description = req.Description?.Trim();
        movie.DurationMinutes = req.DurationMinutes; movie.ReleaseDate = req.ReleaseDate;
        movie.Language = req.Language.Trim(); movie.Genre = req.Genre.Trim();
        await _repo.UpdateAsync(movie);
        return Map(movie);
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Movie with ID {id} not found.");
        if (await _repo.HasShowsAsync(id)) throw new ConflictException("Cannot delete movie with scheduled shows.");
        await _repo.DeleteAsync(movie);
    }

    private static MovieResponse Map(Movie m) => new()
    {
        MovieId = m.MovieId, Title = m.Title, Description = m.Description,
        DurationMinutes = m.DurationMinutes, ReleaseDate = m.ReleaseDate,
        Language = m.Language, Genre = m.Genre
    };
}
