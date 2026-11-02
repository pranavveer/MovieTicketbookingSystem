using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class ScreenService : IScreenService
{
    private readonly IScreenRepository _screenRepo;
    private readonly ITheatreRepository _theatreRepo;

    public ScreenService(IScreenRepository screenRepo, ITheatreRepository theatreRepo)
    {
        _screenRepo = screenRepo; _theatreRepo = theatreRepo;
    }

    public async Task<IEnumerable<ScreenResponse>> GetAllAsync(int? theatreId = null) =>
        (await _screenRepo.GetAllAsync(theatreId)).Select(Map);

    public async Task<ScreenResponse> GetByIdAsync(int id)
    {
        var s = await _screenRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Screen with ID {id} not found.");
        return Map(s);
    }

    public async Task<IEnumerable<ScreenResponse>> GetByTheatreIdAsync(int theatreId)
    {
        if (await _theatreRepo.GetByIdAsync(theatreId) == null) throw new NotFoundException($"Theatre with ID {theatreId} not found.");
        return (await _screenRepo.GetByTheatreIdAsync(theatreId)).Select(Map);
    }

    public async Task<ScreenResponse> CreateAsync(CreateScreenRequest req)
    {
        var theatre = await _theatreRepo.GetByIdAsync(req.TheatreId) ?? throw new NotFoundException($"Theatre with ID {req.TheatreId} not found.");
        if (await _screenRepo.ExistsByNameAsync(req.TheatreId, req.ScreenName.Trim()))
            throw new ConflictException($"Screen '{req.ScreenName}' already exists in this theatre.");
        var screen = new Screen { TheatreId = req.TheatreId, ScreenName = req.ScreenName.Trim() };
        var created = await _screenRepo.AddAsync(screen);
        created.Theatre = theatre;
        return Map(created);
    }

    public async Task<ScreenResponse> UpdateAsync(int id, UpdateScreenRequest req)
    {
        var screen = await _screenRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Screen with ID {id} not found.");
        if (await _screenRepo.ExistsByNameAsync(screen.TheatreId, req.ScreenName.Trim(), id))
            throw new ConflictException($"Screen '{req.ScreenName}' already exists in this theatre.");
        screen.ScreenName = req.ScreenName.Trim();
        await _screenRepo.UpdateAsync(screen);
        return Map(screen);
    }

    public async Task DeleteAsync(int id)
    {
        var screen = await _screenRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Screen with ID {id} not found.");
        if (await _screenRepo.HasSeatsOrShowsAsync(id)) throw new ConflictException("Cannot delete screen with existing seats or shows.");
        await _screenRepo.DeleteAsync(screen);
    }

    private static ScreenResponse Map(Screen s) => new()
    {
        ScreenId = s.ScreenId, TheatreId = s.TheatreId,
        TheatreName = s.Theatre?.Name ?? string.Empty, ScreenName = s.ScreenName,
        TotalSeats = s.Seats?.Count ?? 0
    };
}
