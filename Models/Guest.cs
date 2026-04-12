using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace Plan2Gather.Models;

[PrimaryKey(nameof(GuestName), nameof(EventId))]
public class Guest
{
    [Required, MinLength(1), MaxLength(16), Key] public required string GuestName { get; set; }
    [Required, Key] public int EventId { get; set; }
    [RegularExpression("^([a-z0-9]{1,6}[|#])+[0-9]+$")] public required string Times { get; set; }
    public string? PasswordHash { get; set; }
}
