using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Theatres;

public class UpdateTheatreRequest
{
    [Required][StringLength(150, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    [Required][StringLength(200, MinimumLength = 1)]
    public string Location { get; set; } = string.Empty;
}
