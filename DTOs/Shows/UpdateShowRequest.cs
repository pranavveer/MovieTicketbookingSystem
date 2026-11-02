using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Shows;

public class UpdateShowRequest
{
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    [Required][Range(0, 100000)]
    public decimal TicketPrice { get; set; }
}
