using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Shows;

public class CreateShowRequest
{
    [Required]
    public int MovieId { get; set; }
    [Required]
    public int ScreenId { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    [Required][Range(0, 100000)]
    public decimal TicketPrice { get; set; }
}
