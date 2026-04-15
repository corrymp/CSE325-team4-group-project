using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan2Gather.Models;

public class EventTimeSlot
{
    [Key]
    public int Id { get; set; }

    public int EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public Event? Event { get; set; }

    public DayOfWeek DayOfWeek { get; set; } // Monday = 1
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}