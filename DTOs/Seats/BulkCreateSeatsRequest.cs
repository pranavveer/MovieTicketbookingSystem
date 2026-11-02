using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Seats;

public class BulkCreateSeatsRequest
{
    [Required]
    public int ScreenId { get; set; }
    [Required][MinLength(1)]
    public List<SeatItemRequest> Seats { get; set; } = new();
}

public class SeatItemRequest
{
    [Required][StringLength(5, MinimumLength = 1)]
    public string RowName { get; set; } = string.Empty;
    [Required][Range(1, int.MaxValue)]
    public int SeatNumber { get; set; }
    [Required][StringLength(30, MinimumLength = 1)]
    public string SeatType { get; set; } = "Regular";
}
