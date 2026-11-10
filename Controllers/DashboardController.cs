using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

/// <summary>
/// Accesul se verifică pe server, înainte de generarea paginii:
/// nelogat → /autentificare, alt rol → /acces-interzis.
/// </summary>
public class DashboardController : Controller
{
    [Authorize(Roles = AppRoles.Client)]
    [HttpGet("cont")]
    public IActionResult Client() => View();

    [Authorize(Roles = AppRoles.Controlor)]
    [HttpGet("validare")]
    public async Task<IActionResult> Controlor([FromServices] IValidareService validareService)
    {
        var controlorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var model = new ValidareViewModel
        {
            ValidariAzi = await validareService.ObtineValidariAziAsync(controlorId)
        };

        return View(model);
    }

    [Authorize(Roles = AppRoles.Administrator)]
    [HttpGet("admin")]
    public IActionResult Admin() => View();
}
