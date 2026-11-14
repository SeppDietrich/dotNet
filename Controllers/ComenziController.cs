using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Client)]
[Route("comenzi")]
public class ComenziController(IAbonamentService abonamentService) : Controller
{
    [HttpGet("")]
    public IActionResult Index() => RedirectToAction("Client", "Dashboard");

    [HttpPost("plaseaza")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaseazaComanda(PlasareComandaViewModel model)
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        if (!ModelState.IsValid)
        {
            var primaEroare = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            TempData["MsgErr"] = primaEroare ?? "Datele introduse pentru comandă nu sunt valide.";
            return RedirectToAction("Client", "Dashboard");
        }

        var (succes, eroare, abonament) = await abonamentService.CreeazaAbonamentAsync(
            clientId,
            model.TipAbonamentId,
            model.CategorieId,
            model.DurataLuni,
            model.DataInceput);

        if (!succes)
        {
            TempData["MsgErr"] = eroare ?? "Comanda nu a putut fi procesată.";
            return RedirectToAction("Client", "Dashboard");
        }

        TempData["MsgOk"] = $"Comanda a fost confirmată! Codul abonamentului tău este {abonament!.Cod}.";
        return RedirectToAction("Client", "Dashboard");
    }

    [HttpPost("anuleaza")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anuleaza(int id)
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var (succes, eroare, cod) = await abonamentService.AnuleazaAbonamentAsync(clientId, id);
        if (!succes)
        {
            TempData["MsgErr"] = eroare ?? "Abonamentul nu a putut fi anulat.";
            return RedirectToAction("Client", "Dashboard");
        }

        TempData["MsgOk"] = $"Abonamentul {cod} a fost anulat cu succes.";
        return RedirectToAction("Client", "Dashboard");
    }
}
