using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace Plan2Gather.Models;

/**
 * A guest is defined by an event, a username, and a set of times the guest is available for.
 * They may optionaly include a password if desired.
 * The table uses a joint key of the event ID and guest name, as guest names must be unique per-event but may be duplicated between events.
 * For a detailed overview of the times string, refer to the Availabilities model.
 */

[PrimaryKey(nameof(GuestName), nameof(EventId))]
public class Guest
{
    [Required, MinLength(1), MaxLength(16), Key] public required string GuestName { get; set; }
    [Required, Key] public int EventId { get; set; }
    [RegularExpression("^([a-z0-9]{1,6}[|#])+[0-9]+$")] public required string Times { get; set; }
    public string? PasswordHash { get; set; }
}
