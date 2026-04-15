using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plan2Gather.Data;
using Plan2Gather.Models;

namespace Plan2Gather.Services;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events").RequireAuthorization();

        // DISABLE ANTIFORGERY for the create endpoint
        group.MapPost("/", CreateEvent).DisableAntiforgery();
        group.MapGet("/{eventId}", GetEvent);
        group.MapPost("/{eventId}/attendance", SetAvailability);
        group.MapGet("/{eventId}/attendances", GetAttendances);
        group.MapDelete("/{eventId}/users/{userId}", RemoveUser);
        group.MapPost("/{eventId}/dummy-user", AddDummyUser);
        group.MapPut("/{eventId}/allow-guests", ToggleGuestAccounts);
        group.MapGet("/{eventId}/export", ExportAvailability);
    }

    private static async Task<IResult> CreateEvent(
        HttpContext http,
        Plan2GatherContext db,
        [FromBody] CreateEventRequest req)
    {
        var userIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Results.Unauthorized();

        if (!int.TryParse(userIdClaim, out var organizerId))
            return Results.Unauthorized();

        var user = await db.Users.FindAsync(organizerId);
        if (user == null) return Results.Unauthorized();

        var eventEntity = new Event
        {
            OrganizerId = organizerId,
            EventName = req.Name,
            EventDescription = req.Description ?? "",
            StartTime = DateTime.UtcNow,
            Duration = 0,
            AllowGuests = req.AllowGuestAccounts
        };

        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        foreach (var slotReq in req.TimeSlots)
        {
            var slot = new EventTimeSlot
            {
                EventId = eventEntity.EventId,
                DayOfWeek = slotReq.DayOfWeek,
                StartTime = TimeOnly.Parse(slotReq.StartTime),
                EndTime = TimeOnly.Parse(slotReq.EndTime)
            };
            db.EventTimeSlots.Add(slot);
        }
        await db.SaveChangesAsync();

        return Results.Created($"/api/events/{eventEntity.EventId}", new { eventId = eventEntity.EventId });
    }

    private static async Task<IResult> GetEvent(
        int eventId,
        Plan2GatherContext db,
        HttpContext http)
    {
        var ev = await db.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (ev == null) return Results.NotFound();

        var userIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = userIdClaim != null ? int.Parse(userIdClaim) : null;
        bool isOrganizer = currentUserId.HasValue && currentUserId == ev.OrganizerId;

        var timeSlots = await db.EventTimeSlots
            .Where(ts => ts.EventId == eventId)
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.StartTime)
            .ToListAsync();

        var organizer = await db.Users.FindAsync(ev.OrganizerId);
        var organizerName = organizer?.UserName ?? "Unknown";

        return Results.Ok(new
        {
            ev.EventId,
            ev.EventName,
            ev.EventDescription,
            ev.AllowGuests,
            OrganizerName = organizerName,
            IsOrganizer = isOrganizer,
            TimeSlots = timeSlots.Select(ts => new
            {
                ts.Id,
                ts.DayOfWeek,
                StartTime = ts.StartTime.ToString("HH:mm"),
                EndTime = ts.EndTime.ToString("HH:mm")
            })
        });
    }

    private static async Task<IResult> SetAvailability(
        int eventId,
        HttpContext http,
        Plan2GatherContext db,
        [FromBody] SetAvailabilityRequest req)
    {
        var userIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Results.Unauthorized();

        if (!int.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var ev = await db.Events.FindAsync(eventId);
        if (ev == null) return Results.NotFound();

        var validSlotIds = await db.EventTimeSlots
            .Where(ts => ts.EventId == eventId)
            .Select(ts => ts.Id)
            .ToListAsync();

        if (req.AvailableSlotIds.Any(id => !validSlotIds.Contains(id)))
            return Results.BadRequest("Invalid time slot id");

        var existing = db.EventAttendances
            .Where(a => a.EventId == eventId && a.UserId == userId);
        db.EventAttendances.RemoveRange(existing);
        await db.SaveChangesAsync();

        foreach (var slotId in req.AvailableSlotIds)
        {
            db.EventAttendances.Add(new EventAttendance
            {
                EventId = eventId,
                TimeSlotId = slotId,
                UserId = userId,
                IsAvailable = true
            });
        }
        await db.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<IResult> GetAttendances(
        int eventId,
        Plan2GatherContext db)
    {
        var attendances = await db.EventAttendances
            .Include(a => a.User)
            .Include(a => a.TimeSlot)
            .Where(a => a.EventId == eventId)
            .ToListAsync();

        var result = attendances
            .GroupBy(a => new { a.UserId, a.User!.UserName })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.UserName,
                AvailableSlotIds = g.Select(a => a.TimeSlotId).ToList()
            })
            .ToList();

        return Results.Ok(result);
    }

    private static async Task<IResult> RemoveUser(
        int eventId,
        int userId,
        Plan2GatherContext db,
        HttpContext http)
    {
        var currentUserIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null) return Results.Unauthorized();

        var currentUserId = int.Parse(currentUserIdClaim);
        var ev = await db.Events.FindAsync(eventId);
        if (ev == null) return Results.NotFound();

        if (ev.OrganizerId != currentUserId)
            return Results.Forbid();

        var attendances = db.EventAttendances.Where(a => a.EventId == eventId && a.UserId == userId);
        db.EventAttendances.RemoveRange(attendances);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> AddDummyUser(
        int eventId,
        Plan2GatherContext db,
        HttpContext http)
    {
        var currentUserIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null) return Results.Unauthorized();

        var currentUserId = int.Parse(currentUserIdClaim);
        var ev = await db.Events.FindAsync(eventId);
        if (ev == null) return Results.NotFound();

        if (ev.OrganizerId != currentUserId)
            return Results.Forbid();

        var dummyName = $"Guest_{Guid.NewGuid():N}";
        var dummy = new User
        {
            UserName = dummyName,
            PasswordHash = "dummy",
            UserType = User.UserTypes.BASIC,
        };
        db.Users.Add(dummy);
        await db.SaveChangesAsync();

        var userIdProperty = dummy.GetType().GetProperty("UserId") ?? dummy.GetType().GetProperty("Id");
        var dummyId = userIdProperty?.GetValue(dummy) as int? ?? 0;
        
        return Results.Ok(new { Id = dummyId, UserName = dummy.UserName });
    }

    private static async Task<IResult> ToggleGuestAccounts(
        int eventId,
        bool allow,
        Plan2GatherContext db,
        HttpContext http)
    {
        var currentUserIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null) return Results.Unauthorized();

        var currentUserId = int.Parse(currentUserIdClaim);
        var ev = await db.Events.FindAsync(eventId);
        if (ev == null) return Results.NotFound();

        if (ev.OrganizerId != currentUserId)
            return Results.Forbid();

        ev.AllowGuests = allow;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ExportAvailability(
        int eventId,
        Plan2GatherContext db,
        HttpContext http)
    {
        var currentUserIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null) return Results.Unauthorized();

        var currentUserId = int.Parse(currentUserIdClaim);
        var ev = await db.Events.FindAsync(eventId);
        if (ev == null) return Results.NotFound();

        if (ev.OrganizerId != currentUserId)
            return Results.Forbid();

        var timeSlots = await db.EventTimeSlots
            .Where(ts => ts.EventId == eventId)
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.StartTime)
            .ToListAsync();

        var attendances = await db.EventAttendances
            .Include(a => a.User)
            .Where(a => a.EventId == eventId)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("User,Day,Start Time,End Time,Available");
        foreach (var a in attendances)
        {
            var slot = timeSlots.FirstOrDefault(ts => ts.Id == a.TimeSlotId);
            if (slot != null)
            {
                csv.AppendLine($"{a.User?.UserName},{slot.DayOfWeek},{slot.StartTime},{slot.EndTime},{a.IsAvailable}");
            }
        }
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return Results.File(bytes, "text/csv", $"event_{ev.EventId}_availability.csv");
    }
}

public record CreateEventRequest(
    string Name,
    string? Description,
    bool AllowGuestAccounts,
    List<TimeSlotRequest> TimeSlots
);

public record TimeSlotRequest(
    DayOfWeek DayOfWeek,
    string StartTime,
    string EndTime
);

public record SetAvailabilityRequest(List<int> AvailableSlotIds);