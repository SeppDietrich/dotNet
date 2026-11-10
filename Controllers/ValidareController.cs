using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Controlor)]
public class ValidareController(IValidareService validareService) : Controller
{
    [HttpPost("validare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verifica(ValidareViewModel model)
    {
        var controlorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        if (string.IsNullOrWhiteSpace(model.Cod))
        {
            ModelState.AddModelError(nameof(model.Cod), "Introdu codul abonamentului.");
        }

        if (!ModelState.IsValid)
        {
            model.ValidariAzi = await validareService.ObtineValidariAziAsync(controlorId);
            return View("~/Views/Dashboard/Controlor.cshtml", model);
        }

        var rezultat = await validareService.VerificaCodAsync(controlorId, model.Cod);
        var raspuns = new ValidareViewModel
        {
            Cod = "",
            RezultatCurent = rezultat,
            ValidariAzi = await validareService.ObtineValidariAziAsync(controlorId)
        };

        ModelState.Clear();
        return View("~/Views/Dashboard/Controlor.cshtml", raspuns);
    }
}
