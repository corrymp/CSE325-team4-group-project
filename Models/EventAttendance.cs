using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan2Gather.Models;

public class EventAttendance
{
    [Key]
    public int Id { get; set; }

    public int EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public Event? Event { get; set; }

    public int TimeSlotId { get; set; }
    [ForeignKey(nameof(TimeSlotId))]
    public EventTimeSlot? TimeSlot { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public bool IsAvailable { get; set; } = true;
}