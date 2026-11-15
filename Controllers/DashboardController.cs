using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

/// <summary>
/// Accesul se verifică pe server, înainte de generarea paginii:
/// nelogat → /autentificare, alt rol → /acces-interzis.
/// </summary>
public class DashboardController(AppDbContext db) : Controller
{
    [Authorize(Roles = AppRoles.Client)]
    [HttpGet("cont")]
    public async Task<IActionResult> Client([FromServices] IAbonamentService abonamentService)
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var azi = DateOnly.FromDateTime(DateTime.UtcNow);

        var model = new ClientDashboardViewModel
        {
            Abonamente = await abonamentService.ObtineAbonamenteClientAsync(clientId),
            TipuriDisponibile = await abonamentService.ObtineTipuriActiveAsync(),
            CategoriiDisponibile = await abonamentService.ObtineCategoriiActiveAsync(),
            FormularComanda = new PlasareComandaViewModel { DataInceput = azi }
        };

        return View(model);
    }

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
    public async Task<IActionResult> Admin()
    {
        var azi = DateOnly.FromDateTime(DateTime.UtcNow);
        var inceputAziUtc = DateTime.UtcNow.Date;
        var sfarsitAziUtc = inceputAziUtc.AddDays(1);
        var acumSapteZileUtc = inceputAziUtc.AddDays(-7);

        var model = new AdminDashboardViewModel
        {
            AbonamenteActive = await db.Abonamente.CountAsync(a =>
                a.Stare == StareAbonament.Activ &&
                a.DataInceput <= azi &&
                a.DataExpirare >= azi),
            ComenziUltimaSaptamana = await db.Comenzi.CountAsync(c =>
                c.DataComenzii >= acumSapteZileUtc),
            ValidariAzi = await db.Controale.CountAsync(c =>
                c.DataOra >= inceputAziUtc &&
                c.DataOra < sfarsitAziUtc),
            NumarConturi = await db.Users.CountAsync()
        };

        return View(model);
    }
}
