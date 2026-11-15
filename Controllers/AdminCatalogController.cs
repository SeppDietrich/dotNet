using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
[Route("admin/catalog")]
public class AdminCatalogController(AppDbContext db, IJurnalService jurnalService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await CreeazaModelAsync());

    [HttpPost("categorie")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdaugaCategorie([Bind(Prefix = "FormularCategorie")] CategorieFormViewModel formular)
    {
        var nume = formular.Nume?.Trim() ?? "";
        if (ModelState.IsValid && await NumeExistentAsync(nume, null))
        {
            ModelState.AddModelError("FormularCategorie.Nume", $"Există deja o categorie cu numele „{nume}”.");
        }

        if (!ModelState.IsValid)
        {
            var model = await CreeazaModelAsync();
            model.FormularCategorie = formular;
            return View("Index", model);
        }

        db.Categorii.Add(new Categorie { Nume = nume, Descriere = formular.Descriere?.Trim() ?? "", Activ = true });
        await db.SaveChangesAsync();
        await LogAsync("Adăugare categorie", nume);

        TempData["MsgOk"] = $"Categoria „{nume}” a fost adăugată.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categorie/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificaCategorie(int id, CategorieFormViewModel formular)
    {
        var categorie = await db.Categorii.FindAsync(id);
        if (categorie is null) return NotFound();

        var nume = formular.Nume?.Trim() ?? "";
        if (!ModelState.IsValid)
        {
            TempData["MsgErr"] = PrimaEroare();
            return RedirectToAction(nameof(Index));
        }

        if (await NumeExistentAsync(nume, id))
        {
            TempData["MsgErr"] = $"Există deja o categorie cu numele „{nume}”.";
            return RedirectToAction(nameof(Index));
        }

        categorie.Nume = nume;
        categorie.Descriere = formular.Descriere?.Trim() ?? "";
        await db.SaveChangesAsync();
        await LogAsync("Modificare categorie", nume);

        TempData["MsgOk"] = $"Categoria „{nume}” a fost actualizată.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categorie/{id:int}/stare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SchimbaStareCategorie(int id)
    {
        var categorie = await db.Categorii.FindAsync(id);
        if (categorie is null) return NotFound();

        categorie.Activ = !categorie.Activ;
        await db.SaveChangesAsync();
        await LogAsync(categorie.Activ ? "Reactivare categorie" : "Dezactivare categorie", categorie.Nume);

        TempData["MsgOk"] = $"Categoria „{categorie.Nume}” a fost {(categorie.Activ ? "reactivată" : "dezactivată")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("tip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdaugaTip([Bind(Prefix = "FormularTip")] TipAbonamentFormViewModel formular)
    {
        Categorie? categorie = null;
        if (ModelState.IsValid)
        {
            categorie = await db.Categorii.FindAsync(formular.CategorieId!.Value);
            if (categorie is null)
            {
                ModelState.AddModelError("FormularTip.CategorieId", "Categoria aleasă nu există.");
            }
            else if (await TipExistentAsync(categorie.Id, formular.DurataLuni!.Value))
            {
                ModelState.AddModelError("FormularTip.DurataLuni", MesajDuplicat(categorie.Nume, formular.DurataLuni!.Value));
            }
        }

        if (!ModelState.IsValid || categorie is null)
        {
            var model = await CreeazaModelAsync();
            model.FormularTip = formular;
            return View("Index", model);
        }

        var tip = new TipAbonament
        {
            CategorieId = categorie.Id,
            DurataLuni = formular.DurataLuni!.Value,
            Pret = formular.Pret!.Value,
            Activ = true
        };
        db.TipuriAbonament.Add(tip);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["MsgErr"] = MesajDuplicat(categorie.Nume, tip.DurataLuni);
            return RedirectToAction(nameof(Index));
        }

        var descriere = $"{categorie.Nume}, {Tarife.EticheteDurata(tip.DurataLuni)}, {tip.Pret} MDL";
        await LogAsync("Adăugare tip abonament", descriere);

        TempData["MsgOk"] = $"Tipul de abonament ({descriere}) a fost adăugat.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("tip/{id:int}/pret")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificaPret(int id, PretFormViewModel formular)
    {
        var tip = await db.TipuriAbonament.Include(t => t.Categorie).FirstOrDefaultAsync(t => t.Id == id);
        if (tip is null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["MsgErr"] = PrimaEroare();
            return RedirectToAction(nameof(Index));
        }

        var vechi = tip.Pret;
        tip.Pret = formular.Pret!.Value;
        await db.SaveChangesAsync();
        await LogAsync("Modificare preț", $"{tip.Categorie.Nume}, {Tarife.EticheteDurata(tip.DurataLuni)}: {vechi} → {tip.Pret} MDL");

        TempData["MsgOk"] = $"Prețul pentru {tip.Categorie.Nume}, {Tarife.EticheteDurata(tip.DurataLuni)} a fost actualizat.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("tip/{id:int}/stare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SchimbaStareTip(int id)
    {
        var tip = await db.TipuriAbonament.Include(t => t.Categorie).FirstOrDefaultAsync(t => t.Id == id);
        if (tip is null) return NotFound();

        tip.Activ = !tip.Activ;
        await db.SaveChangesAsync();

        var descriere = $"{tip.Categorie.Nume}, {Tarife.EticheteDurata(tip.DurataLuni)}";
        await LogAsync(tip.Activ ? "Reactivare tip abonament" : "Dezactivare tip abonament", descriere);

        TempData["MsgOk"] = $"Tipul de abonament ({descriere}) a fost {(tip.Activ ? "reactivat" : "dezactivat")}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminCatalogViewModel> CreeazaModelAsync() => new()
    {
        Categorii = await db.Categorii
            .AsNoTracking()
            .Include(c => c.Tipuri.OrderBy(t => t.DurataLuni))
            .OrderBy(c => c.Id)
            .ToListAsync()
    };

    private Task<bool> NumeExistentAsync(string nume, int? exceptId) =>
        db.Categorii.AnyAsync(c => c.Nume.ToLower() == nume.ToLower() && c.Id != exceptId);

    private Task<bool> TipExistentAsync(int categorieId, int durataLuni) =>
        db.TipuriAbonament.AnyAsync(t => t.CategorieId == categorieId && t.DurataLuni == durataLuni);

    private static string MesajDuplicat(string categorie, int luni) =>
        $"Categoria „{categorie}” are deja un tip de {Tarife.EticheteDurata(luni)}. Modifică prețul sau reactivează tipul existent.";

    private string PrimaEroare() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault() ?? "Date invalide.";

    private Task LogAsync(string actiune, string obiect) =>
        jurnalService.LogAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), actiune, obiect);
}
