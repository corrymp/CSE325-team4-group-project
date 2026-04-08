using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HangoutPlanner.Controllers;

[Authorize]
public class UserController : Controller
{
    public IActionResult Profile()
    {
        return Content("User weekly availability grid will go here.");
    }
}