using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace Plan2Gather.Models;

[PrimaryKey(nameof(UserId), nameof(EventId))]
public class Availability
{
    [Required, Key] public int UserId { get; set; }
    [Required, Key] public int EventId { get; set; }
    [Required, RegularExpression("^([a-z0-9]{1,6}[|#])+[0-9]+$")] public required string Times { get; set; }
}
