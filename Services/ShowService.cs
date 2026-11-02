using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class ShowService : IShowService
{
    private readonly IShowRepository _showRepo;
    private readonly IMovieRepository _movieRepo;
    private readonly IScreenRepository _screenRepo;
    private readonly ISeatRepository _seatRepo;

    public ShowService(IShowRepository showRepo, IMovieRepository movieRepo, IScreenRepository screenRepo, ISeatRepository seatRepo)
    {
        _showRepo = showRepo; _movieRepo = movieRepo; _screenRepo = screenRepo; _seatRepo = seatRepo;
    }

    public async Task<IEnumerable<ShowResponse>> GetAllAsync(DateTime? date = null, int? movieId = null, int? theatreId = null) =>
        (await _showRepo.GetAllAsync(date, movieId, theatreId)).Select(Map);

    public async Task<ShowResponse> GetByIdAsync(int id)
    {
        var show = await _showRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Show with ID {id} not found.");
        return Map(show);
    }

    public async Task<IEnumerable<ShowResponse>> GetByMovieIdAsync(int movieId)
    {
        if (await _movieRepo.GetByIdAsync(movieId) == null) throw new NotFoundException($"Movie with ID {movieId} not found.");
        return (await _showRepo.GetByMovieIdAsync(movieId)).Select(Map);
    }

    public async Task<IEnumerable<ShowResponse>> GetByTheatreIdAsync(int theatreId) =>
        (await _showRepo.GetByTheatreIdAsync(theatreId)).Select(Map);

    public async Task<IEnumerable<ShowResponse>> SearchByDateAsync(DateOnly date) =>
        (await _showRepo.SearchByDateAsync(date)).Select(Map);

    public async Task<ShowResponse> CreateAsync(CreateShowRequest req)
    {
        if (req.EndTime <= req.StartTime) throw new BadRequestException("EndTime must be after StartTime.");
        if (req.TicketPrice < 0) throw new BadRequestException("TicketPrice cannot be negative.");

        var movie = await _movieRepo.GetByIdAsync(req.MovieId) ?? throw new NotFoundException($"Movie with ID {req.MovieId} not found.");
        var screen = await _screenRepo.GetByIdAsync(req.ScreenId) ?? throw new NotFoundException($"Screen with ID {req.ScreenId} not found.");

        if (await _showRepo.HasOverlappingShowAsync(req.ScreenId, req.StartTime, req.EndTime))
            throw new ConflictException("Screen is already booked for another show during this time.");

        var show = new Show { MovieId = req.MovieId, ScreenId = req.ScreenId, StartTime = req.StartTime, EndTime = req.EndTime, TicketPrice = req.TicketPrice };
        var created = await _showRepo.AddAsync(show);

        // Auto-generate ShowSeats for every physical seat on this screen
        var physicalSeats = (await _seatRepo.GetByScreenIdAsync(req.ScreenId)).ToList();
        if (physicalSeats.Any())
        {
            var showSeats = physicalSeats.Select(s => new ShowSeat { ShowId = created.ShowId, SeatId = s.SeatId, Status = "Available" }).ToList();
            await _showRepo.AddShowSeatsAsync(showSeats);
            created.ShowSeats = showSeats;
        }

        created.Movie = movie; created.Screen = screen;
        return Map(created);
    }

    public async Task<ShowResponse> UpdateAsync(int id, UpdateShowRequest req)
    {
        var show = await _showRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Show with ID {id} not found.");
        if (req.EndTime <= req.StartTime) throw new BadRequestException("EndTime must be after StartTime.");
        if (req.TicketPrice < 0) throw new BadRequestException("TicketPrice cannot be negative.");
        if (await _showRepo.HasOverlappingShowAsync(show.ScreenId, req.StartTime, req.EndTime, id))
            throw new ConflictException("Screen is already booked for another show during this time.");
        show.StartTime = req.StartTime; show.EndTime = req.EndTime; show.TicketPrice = req.TicketPrice;
        await _showRepo.UpdateAsync(show);
        return Map(show);
    }

    public async Task DeleteAsync(int id)
    {
        var show = await _showRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Show with ID {id} not found.");
        if (await _showRepo.HasBookingsAsync(id)) throw new ConflictException("Cannot delete show with existing bookings.");
        await _showRepo.DeleteAsync(show);
    }

    public async Task<IEnumerable<ShowSeatResponse>> GetShowSeatsAsync(int showId, string? statusFilter = null)
    {
        var show = await _showRepo.GetByIdAsync(showId) ?? throw new NotFoundException($"Show with ID {showId} not found.");
        var showSeats = await _showRepo.GetShowSeatsAsync(showId, statusFilter);
        return showSeats.Select(ss => new ShowSeatResponse
        {
            SeatId = ss.SeatId, RowName = ss.Seat?.RowName ?? string.Empty,
            SeatNumber = ss.Seat?.SeatNumber ?? 0, SeatType = ss.Seat?.SeatType ?? string.Empty,
            Price = show.TicketPrice, Status = ss.Status
        });
    }

    private static ShowResponse Map(Show s) => new()
    {
        ShowId = s.ShowId, MovieId = s.MovieId, MovieTitle = s.Movie?.Title ?? string.Empty,
        ScreenId = s.ScreenId, ScreenName = s.Screen?.ScreenName ?? string.Empty,
        TheatreId = s.Screen?.TheatreId ?? 0, TheatreName = s.Screen?.Theatre?.Name ?? string.Empty,
        StartTime = s.StartTime, EndTime = s.EndTime, TicketPrice = s.TicketPrice,
        TotalSeats = s.ShowSeats?.Count ?? 0,
        AvailableSeats = s.ShowSeats?.Count(ss => ss.Status.Equals("Available", StringComparison.OrdinalIgnoreCase)) ?? 0
    };
}
