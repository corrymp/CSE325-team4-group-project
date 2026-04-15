using System.ComponentModel.DataAnnotations;
namespace Plan2Gather.Models;

/** 
 * Each user is defined by an ID, a user type, and a name, and a password, and optionally may include an email.
 * The user type is determined at account creation by whether or not the user is setting an email.
 * 
 * Only full-priveledge accounts may create events, which means only accounts that include an email may create events.
 * Usernames are not required to be unique if the user provides an email, but only one non-email-having account may have a given name.
 * Example: any number of full accounts may be named 'Alex', but there may only be a single basic account with that name.
 */
public class User
{
    public enum UserTypes { NONE, BASIC, FULL }
    public int UserId { get; set; }
    [Required, MinLength(1), MaxLength(16)] public required string UserName { get; set; }
    public string? PasswordHash { get; set; }
    [Required] public UserTypes UserType { get; set; }
    [EmailAddress] public string? Email { get; set; }
}
