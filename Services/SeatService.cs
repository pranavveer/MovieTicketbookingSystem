using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepo;
    private readonly IScreenRepository _screenRepo;

    public SeatService(ISeatRepository seatRepo, IScreenRepository screenRepo)
    {
        _seatRepo = seatRepo; _screenRepo = screenRepo;
    }

    public async Task<IEnumerable<SeatResponse>> GetByScreenIdAsync(int screenId)
    {
        if (await _screenRepo.GetByIdAsync(screenId) == null) throw new NotFoundException($"Screen with ID {screenId} not found.");
        return (await _seatRepo.GetByScreenIdAsync(screenId)).Select(Map);
    }

    public async Task<SeatResponse> GetByIdAsync(int id)
    {
        var seat = await _seatRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Seat with ID {id} not found.");
        return Map(seat);
    }

    public async Task<SeatResponse> CreateAsync(CreateSeatRequest req)
    {
        if (await _screenRepo.GetByIdAsync(req.ScreenId) == null) throw new NotFoundException($"Screen with ID {req.ScreenId} not found.");
        var row = req.RowName.Trim().ToUpper();
        if (await _seatRepo.ExistsAsync(req.ScreenId, row, req.SeatNumber))
            throw new ConflictException($"Seat {row}{req.SeatNumber} already exists in screen {req.ScreenId}.");
        var seat = new Seat { ScreenId = req.ScreenId, RowName = row, SeatNumber = req.SeatNumber, SeatType = req.SeatType.Trim() };
        return Map(await _seatRepo.AddAsync(seat));
    }

    public async Task<IEnumerable<SeatResponse>> BulkCreateAsync(BulkCreateSeatsRequest req)
    {
        if (await _screenRepo.GetByIdAsync(req.ScreenId) == null) throw new NotFoundException($"Screen with ID {req.ScreenId} not found.");
        if (req.Seats == null || !req.Seats.Any()) throw new BadRequestException("At least one seat must be provided.");

        var dups = req.Seats.GroupBy(s => new { Row = s.RowName.Trim().ToUpper(), s.SeatNumber })
            .Where(g => g.Count() > 1).Select(g => $"{g.Key.Row}{g.Key.SeatNumber}").ToList();
        if (dups.Any()) throw new BadRequestException($"Payload contains duplicate seats: {string.Join(", ", dups)}.");

        var existing = (await _seatRepo.GetByScreenIdAsync(req.ScreenId))
            .Select(s => $"{s.RowName.ToUpper()}{s.SeatNumber}").ToHashSet();
        var conflicts = req.Seats.Where(s => existing.Contains($"{s.RowName.Trim().ToUpper()}{s.SeatNumber}"))
            .Select(s => $"{s.RowName.Trim().ToUpper()}{s.SeatNumber}").ToList();
        if (conflicts.Any()) throw new ConflictException($"Seats already exist: {string.Join(", ", conflicts)}.");

        var seats = req.Seats.Select(s => new Seat
        {
            ScreenId = req.ScreenId, RowName = s.RowName.Trim().ToUpper(),
            SeatNumber = s.SeatNumber, SeatType = string.IsNullOrWhiteSpace(s.SeatType) ? "Regular" : s.SeatType.Trim()
        }).ToList();
        await _seatRepo.AddRangeAsync(seats);
        return seats.Select(Map);
    }

    public async Task<SeatResponse> UpdateAsync(int id, UpdateSeatRequest req)
    {
        var seat = await _seatRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Seat with ID {id} not found.");
        var row = req.RowName.Trim().ToUpper();
        if (await _seatRepo.ExistsAsync(seat.ScreenId, row, req.SeatNumber, id))
            throw new ConflictException($"Seat {row}{req.SeatNumber} already exists in screen {seat.ScreenId}.");
        seat.RowName = row; seat.SeatNumber = req.SeatNumber; seat.SeatType = req.SeatType.Trim();
        await _seatRepo.UpdateAsync(seat);
        return Map(seat);
    }

    public async Task DeleteAsync(int id)
    {
        var seat = await _seatRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Seat with ID {id} not found.");
        if (await _seatRepo.HasShowSeatsOrBookingsAsync(id)) throw new ConflictException("Cannot delete seat with show/booking history.");
        await _seatRepo.DeleteAsync(seat);
    }

    private static SeatResponse Map(Seat s) => new()
    {
        SeatId = s.SeatId, ScreenId = s.ScreenId,
        RowName = s.RowName, SeatNumber = s.SeatNumber, SeatType = s.SeatType
    };
}
