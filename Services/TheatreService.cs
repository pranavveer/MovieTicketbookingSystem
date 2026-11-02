using MovieTicketBooking.DTOs.Common;
using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class TheatreService : ITheatreService
{
    private readonly ITheatreRepository _repo;
    public TheatreService(ITheatreRepository repo) => _repo = repo;

    public async Task<PagedResult<TheatreResponse>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
    {
        var theatres = await _repo.GetAllAsync(pageNumber, pageSize);
        var total = await _repo.GetCountAsync();
        return new PagedResult<TheatreResponse>
        {
            Items = theatres.Select(Map), TotalCount = total,
            PageNumber = pageNumber ?? 1, PageSize = pageSize ?? total
        };
    }

    public async Task<TheatreResponse> GetByIdAsync(int id)
    {
        var t = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Theatre with ID {id} not found.");
        return Map(t);
    }

    public async Task<TheatreResponse> CreateAsync(CreateTheatreRequest req)
    {
        var theatre = new Theatre { Name = req.Name.Trim(), Location = req.Location.Trim() };
        return Map(await _repo.AddAsync(theatre));
    }

    public async Task<TheatreResponse> UpdateAsync(int id, UpdateTheatreRequest req)
    {
        var t = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Theatre with ID {id} not found.");
        t.Name = req.Name.Trim(); t.Location = req.Location.Trim();
        await _repo.UpdateAsync(t);
        return Map(t);
    }

    public async Task DeleteAsync(int id)
    {
        var t = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Theatre with ID {id} not found.");
        if (await _repo.HasScreensAsync(id)) throw new ConflictException("Cannot delete theatre with existing screens.");
        await _repo.DeleteAsync(t);
    }

    private static TheatreResponse Map(Theatre t) => new()
    {
        TheatreId = t.TheatreId, Name = t.Name, Location = t.Location, ScreenCount = t.Screens.Count
    };
}
