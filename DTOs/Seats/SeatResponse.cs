namespace MovieTicketBooking.DTOs.Seats;

public class SeatResponse
{
    public int SeatId { get; set; }
    public int ScreenId { get; set; }
    public string RowName { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatLabel => $"{RowName}{SeatNumber}";
    public string SeatType { get; set; } = string.Empty;
}
