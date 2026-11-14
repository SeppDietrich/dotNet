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
    private const string MesajTipDuplicat =
        "Există deja un tip de abonament pentru această categorie și durată (dacă este inactiv, reactivează-l din listă).";

    private string? AdminId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await ConstruiesteModelAsync());

    // ---------- Categorii ----------

    [HttpPost("categorie")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreeazaCategorie([Bind(Prefix = "FormCategorie")] CategorieFormViewModel formular)
    {
        var nume = formular.Nume?.Trim() ?? "";

        if (ModelState.IsValid && await ExistaNumeCategorieAsync(nume, null))
        {
            ModelState.AddModelError("FormCategorie.Nume", "Există deja o categorie cu acest nume.");
        }

        if (!ModelState.IsValid)
        {
            return View("Index", await ConstruiesteModelAsync(formCategorie: formular));
        }

        var categorie = new Categorie
        {
            Nume = nume,
            Descriere = formular.Descriere?.Trim() ?? "",
            Activ = true
        };
        db.Categorii.Add(categorie);
        await db.SaveChangesAsync();

        await jurnalService.LogAsync(AdminId, "Creare categorie", categorie.Nume);
        TempData["MsgOk"] = $"Categoria „{categorie.Nume}” a fost adăugată.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categorie/modifica")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult] ModificaCategorie(CategorieEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Eroare(ErorileModelului());
        }

        var categorie = await db.Categorii.FindAsync(model.Id);
        if (categorie is null)
        {
            return Eroare("Categoria nu a fost găsită.");
        }

        var nume = model.Nume.Trim();
        if (await ExistaNumeCategorieAsync(nume, categorie.Id))
        {
            return Eroare("Există deja o categorie cu acest nume.");
        }

        var numeVechi = categorie.Nume;
        categorie.Nume = nume;
        categorie.Descriere = model.Descriere?.Trim() ?? "";
        await db.SaveChangesAsync();

        await jurnalService.LogAsync(AdminId, "Modificare categorie",
            numeVechi == nume ? nume : $"{numeVechi} → {nume}");
        TempData["MsgOk"] = $"Categoria „{categorie.Nume}” a fost actualizată.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categorie/stare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ComutaCategorie(int id)
    {
        var categorie = await db.Categorii.FindAsync(id);
        if (categorie is null)
        {
            return Eroare("Categoria nu a fost găsită.");
        }

        categorie.Activ = !categorie.Activ;
        await db.SaveChangesAsync();

        await jurnalService.LogAsync(AdminId, categorie.Activ ? "Reactivare categorie" : "Dezactivare categorie", categorie.Nume);
        TempData["MsgOk"] = categorie.Activ
            ? $"Categoria „{categorie.Nume}” a fost reactivată."
            : $"Categoria „{categorie.Nume}” a fost dezactivată.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Tipuri de abonament ----------

    [HttpPost("tip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreeazaTip([Bind(Prefix = "FormTip")] TipFormViewModel formular)
    {
        Categorie? categorie = null;

        if (ModelState.IsValid)
        {
            categorie = await db.Categorii.FindAsync(formular.CategorieId);
            if (categorie is null)
            {
                ModelState.AddModelError("FormTip.CategorieId", "Categoria aleasă nu există.");
            }
            else if (await db.TipuriAbonament.AnyAsync(t =>
                         t.CategorieId == categorie.Id && t.DurataLuni == formular.DurataLuni))
            {
                ModelState.AddModelError("FormTip.DurataLuni", MesajTipDuplicat);
            }
        }

        if (!ModelState.IsValid)
        {
            return View("Index", await ConstruiesteModelAsync(formTip: formular));
        }

        var tip = new TipAbonament
        {
            CategorieId = categorie!.Id,
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
            // Cursă între două cereri: indexul unic (CategorieId, DurataLuni) a refuzat duplicatul.
            db.Entry(tip).State = EntityState.Detached;
            ModelState.AddModelError("FormTip.DurataLuni", MesajTipDuplicat);
            return View("Index", await ConstruiesteModelAsync(formTip: formular));
        }

        await jurnalService.LogAsync(AdminId, "Creare tip abonament", DescriereTip(categorie.Nume, tip.DurataLuni, tip.Pret));
        TempData["MsgOk"] = $"Tipul {Formatare.Perioada(tip.DurataLuni)} pentru „{categorie.Nume}” a fost adăugat.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("tip/pret")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificaPret(TipPretEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Eroare(ErorileModelului());
        }

        var tip = await db.TipuriAbonament.Include(t => t.Categorie).FirstOrDefaultAsync(t => t.Id == model.Id);
        if (tip is null)
        {
            return Eroare("Tipul de abonament nu a fost găsit.");
        }

        var pretVechi = tip.Pret;
        var pretNou = model.Pret!.Value;
        if (pretVechi == pretNou)
        {
            TempData["MsgOk"] = "Prețul nu s-a schimbat.";
            return RedirectToAction(nameof(Index));
        }

        // Se modifică doar șablonul (TipAbonament). Abonamentele deja emise își păstrează datele.
        tip.Pret = pretNou;
        await db.SaveChangesAsync();

        await jurnalService.LogAsync(AdminId, "Modificare preț",
            $"{tip.Categorie.Nume}, {tip.DurataLuni} luni: {pretVechi} → {pretNou} lei");
        TempData["MsgOk"] = $"Prețul pentru „{tip.Categorie.Nume}” ({Formatare.Perioada(tip.DurataLuni)}) este acum {Formatare.Pret(pretNou)}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("tip/stare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ComutaTip(int id)
    {
        var tip = await db.TipuriAbonament.Include(t => t.Categorie).FirstOrDefaultAsync(t => t.Id == id);
        if (tip is null)
        {
            return Eroare("Tipul de abonament nu a fost găsit.");
        }

        tip.Activ = !tip.Activ;
        await db.SaveChangesAsync();

        await jurnalService.LogAsync(AdminId, tip.Activ ? "Reactivare tip abonament" : "Dezactivare tip abonament",
            DescriereTip(tip.Categorie.Nume, tip.DurataLuni, tip.Pret));
        TempData["MsgOk"] = tip.Activ
            ? $"Tipul {Formatare.Perioada(tip.DurataLuni)} pentru „{tip.Categorie.Nume}” a fost reactivat."
            : $"Tipul {Formatare.Perioada(tip.DurataLuni)} pentru „{tip.Categorie.Nume}” a fost dezactivat.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Ajutoare ----------

    private async Task<bool> ExistaNumeCategorieAsync(string nume, int? exceptId)
    {
        var n = nume.ToLower();
        var except = exceptId ?? 0;
        return await db.Categorii.AnyAsync(c => c.Id != except && c.Nume.ToLower() == n);
    }

    private async Task<AdminCatalogViewModel> ConstruiesteModelAsync(
        CategorieFormViewModel? formCategorie = null,
        TipFormViewModel? formTip = null)
    {
        var categorii = await db.Categorii
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CategorieCatalogItem
            {
                Id = c.Id,
                Nume = c.Nume,
                Descriere = c.Descriere,
                Activ = c.Activ,
                Tipuri = c.Tipuri
                    .OrderBy(t => t.DurataLuni)
                    .Select(t => new TipCatalogItem
                    {
                        Id = t.Id,
                        DurataLuni = t.DurataLuni,
                        Pret = t.Pret,
                        Activ = t.Activ
                    })
                    .ToList()
            })
            .ToListAsync();

        return new AdminCatalogViewModel
        {
            Categorii = categorii,
            FormCategorie = formCategorie ?? new CategorieFormViewModel(),
            FormTip = formTip ?? new TipFormViewModel()
        };
    }

    private IActionResult Eroare(string mesaj)
    {
        TempData["MsgErr"] = mesaj;
        return RedirectToAction(nameof(Index));
    }

    private string ErorileModelului() =>
        string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Distinct());

    private static string DescriereTip(string categorie, int luni, int pret) =>
        $"{categorie}, {luni} luni, {pret} lei";
}