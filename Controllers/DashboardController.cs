using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;

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
    public IActionResult Controlor() => View();

    [Authorize(Roles = AppRoles.Administrator)]
    [HttpGet("admin")]
    public IActionResult Admin() => View();
}
