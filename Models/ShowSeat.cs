namespace MovieTicketBooking.Models;

public class ShowSeat
{
    public int ShowSeatId { get; set; }
    public int ShowId { get; set; }
    public int SeatId { get; set; }
    public string Status { get; set; } = "Available"; // Available or Booked

    public Show Show { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}
