using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Plan2Gather.Data;
using Plan2Gather.Models;
using Plan2Gather.Services;

namespace Plan2Gather.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register", Register).DisableAntiforgery();
        app.MapPost("/api/auth/login", Login).DisableAntiforgery();
    }

    private static async Task<IResult> Register(RegisterRequest req, Plan2GatherContext db, JwtService jwt)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(req.UserName) || req.UserName.Length < 1 || req.UserName.Length > 16)
            return Results.BadRequest("Username must be 1–16 characters.");

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return Results.BadRequest("Password must be at least 6 characters.");

        if (req.Password != req.ConfirmPassword)
            return Results.BadRequest("Passwords do not match.");

        bool taken = await db.Users.AnyAsync(u => u.UserName.ToLower() == req.UserName.ToLower());
        if (taken)
            return Results.BadRequest("Username is already taken.");

        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            bool emailTaken = await db.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == req.Email.ToLower());
            if (emailTaken)
                return Results.BadRequest("An account with that email already exists.");
        }

        if (!Enum.TryParse<User.UserTypes>(req.AccountType, true, out var userType))
            return Results.BadRequest("Invalid account type.");

        if (userType == User.UserTypes.FULL && string.IsNullOrWhiteSpace(req.Email))
            return Results.BadRequest("Email is required for FULL accounts.");

        var user = new User
        {
            UserName = req.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            UserType = userType,
            Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = jwt.GenerateToken(user);
        if (string.IsNullOrWhiteSpace(token))
            return Results.StatusCode(500);

        return Results.Ok(new AuthResponse(token, user.UserName, user.UserType.ToString()));
    }

    private static async Task<IResult> Login(LoginRequest req, Plan2GatherContext db, JwtService jwt)
    {
        if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest("Username and password are required.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == req.UserName.ToLower());

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            return Results.Unauthorized();

        bool valid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!valid)
            return Results.Unauthorized();

        var token = jwt.GenerateToken(user);
        if (string.IsNullOrWhiteSpace(token))
            return Results.StatusCode(500);

        return Results.Ok(new AuthResponse(token, user.UserName, user.UserType.ToString()));
    }
}

public record RegisterRequest(string UserName, string Password, string ConfirmPassword, string AccountType, string? Email);
public record LoginRequest(string UserName, string Password);
public record AuthResponse(string Token, string UserName, string UserType);