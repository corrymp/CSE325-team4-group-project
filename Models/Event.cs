using System.ComponentModel.DataAnnotations;
namespace Plan2Gather.Models;

public class Event
{
    public int EventId { get; set; }
    [Required] public int OrganizerId { get; set; }
    [Required, MinLength(1), MaxLength(48)] public required string EventName { get; set; }
    [MaxLength(256)] public string EventDescription { get; set; } = "";
    [Required] public DateTime StartTime { get; set; }
    [Required] public int Duration {get; set; }
    public bool AllowGuests { get; set; } = true;
}
