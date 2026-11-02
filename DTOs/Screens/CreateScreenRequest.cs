using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Screens;

public class CreateScreenRequest
{
    [Required]
    public int TheatreId { get; set; }
    [Required][StringLength(50, MinimumLength = 1)]
    public string ScreenName { get; set; } = string.Empty;
}
