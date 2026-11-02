using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Bookings;

public class CreateBookingRequest
{
    [Required][StringLength(100, MinimumLength = 1)]
    public string CustomerName { get; set; } = string.Empty;
    [Required][EmailAddress][StringLength(150)]
    public string CustomerEmail { get; set; } = string.Empty;
    [Required][Phone][StringLength(15, MinimumLength = 7)]
    public string CustomerPhone { get; set; } = string.Empty;
    [Required]
    public int ShowId { get; set; }
    [Required][MinLength(1)]
    public List<int> SeatIds { get; set; } = new();
}
