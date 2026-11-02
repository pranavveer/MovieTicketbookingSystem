using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Screens;

public class UpdateScreenRequest
{
    [Required][StringLength(50, MinimumLength = 1)]
    public string ScreenName { get; set; } = string.Empty;
}
