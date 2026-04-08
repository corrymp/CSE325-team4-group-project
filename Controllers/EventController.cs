using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HangoutPlanner.Controllers;

[Authorize]
public class EventController : Controller
{
    public IActionResult Create()
    {
        return Content("Event creation form will go here.");
    }
}