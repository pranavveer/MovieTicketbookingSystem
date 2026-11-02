namespace MovieTicketBooking.DTOs.Shows;

public class ShowResponse
{
    public int ShowId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int ScreenId { get; set; }
    public string ScreenName { get; set; } = string.Empty;
    public int TheatreId { get; set; }
    public string TheatreName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TicketPrice { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
}
