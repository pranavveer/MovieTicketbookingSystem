namespace MovieTicketBooking.DTOs.Screens;

public class ScreenResponse
{
    public int ScreenId { get; set; }
    public int TheatreId { get; set; }
    public string TheatreName { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}
