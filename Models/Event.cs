using System.ComponentModel.DataAnnotations;
namespace Plan2Gather.Models;

/** 
 * An event survey is defined by and ID, an organizer, a name, a start time, and a list of dates.
 * It also may include a description, and defaults to allowing guests if not specified.
 * Dates are stored as a comma-seperated list of dates in YYYY-MM-DD format.
 * 
 *
 */
public class Event
{
    public int EventId { get; set; }
    [Required] public int OrganizerId { get; set; }
    [Required, MinLength(1), MaxLength(48)] public required string EventName { get; set; }
    [MaxLength(256)] public string EventDescription { get; set; } = "";
    [Required] public DateTime StartTime { get; set; }
    // RegExp to ensure dates are in comma-seperated YYYY-MM-DD format. Example: `2025-03-30,2025-03-31,2025-03-31,2025-03-31,2025-04-02,2025-04-03`
    [Required, RegularExpression(@"^(\d{4}-\d{2}-\d{2},)*\d{4}-\d{2}-\d{2}$")] public string Dates {get; set;} = "";
    public bool AllowGuests { get; set; } = true;
}
