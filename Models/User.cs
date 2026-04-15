using System.ComponentModel.DataAnnotations;
namespace Plan2Gather.Models;

public class User
{
    public enum UserTypes { NONE, BASIC, FULL }
    public int UserId { get; set; }
    [Required, MinLength(1), MaxLength(16)] public required string UserName { get; set; }
    public string? PasswordHash { get; set; }
    [Required] public UserTypes UserType { get; set; }
    [EmailAddress] public string? Email { get; set; }
}
