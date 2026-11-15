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
    public async Task<IActionResult> Index([FromQuery] string? cautare) =>
        View(await CreeazaModelAsync(cautare));

    [HttpGet("detalii/{id:int}")]
    public async Task<IActionResult> Detalii(int id)
    {
        var categorie = await db.Categorii
            .AsNoTracking()
            .Include(c => c.Tipuri.OrderBy(t => t.DurataLuni))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categorie is null)
            return NotFoundView(id, "Categoria");

        var tipIds = categorie.Tipuri.Select(t => t.Id).ToList();
        var numarComenzi = await db.Comenzi.CountAsync(c => tipIds.Contains(c.TipAbonamentId));
        var numarAbonamente = await db.Abonamente.CountAsync(a => tipIds.Contains(a.TipAbonamentId));

        return View(new CategorieDetaliiViewModel
        {
            Categorie = categorie,
            NumarComenzi = numarComenzi,
            NumarAbonamente = numarAbonamente
        });
    }

    [HttpGet("editeaza/{id:int}")]
    public async Task<IActionResult> Editeaza(int id)
    {
        var categorie = await db.Categorii.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (categorie is null)
            return NotFoundView(id, "Categoria");

        return View(new CategorieEditViewModel
        {
            Id = categorie.Id,
            Nume = categorie.Nume,
            Descriere = categorie.Descriere,
            Activ = categorie.Activ
        });
    }

    [HttpPost("editeaza/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editeaza(int id, CategorieEditViewModel formular)
    {
        var categorie = await db.Categorii.FindAsync(id);
        if (categorie is null)
            return NotFoundView(id, "Categoria");

        formular.Id = id;
        var nume = formular.Nume?.Trim() ?? "";

        if (ModelState.IsValid && await NumeExistentAsync(nume, id))
        {
            ModelState.AddModelError(nameof(formular.Nume), $"Există deja o categorie cu numele „{nume}”.");
        }

        if (!ModelState.IsValid)
        {
            return View(formular);
        }

        categorie.Nume = nume;
        categorie.Descriere = formular.Descriere?.Trim() ?? "";
        categorie.Activ = formular.Activ;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "A apărut o eroare la salvarea modificărilor în baza de date.");
            return View(formular);
        }

        await LogAsync("Modificare categorie", nume);
        TempData["MsgOk"] = $"Categoria „{nume}” a fost actualizată.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("sterge/{id:int}")]
    public async Task<IActionResult> Sterge(int id)
    {
        var categorie = await db.Categorii
            .AsNoTracking()
            .Include(c => c.Tipuri.OrderBy(t => t.DurataLuni))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categorie is null)
            return NotFoundView(id, "Categoria");

        var tipIds = categorie.Tipuri.Select(t => t.Id).ToList();
        var numarComenzi = await db.Comenzi.CountAsync(c => tipIds.Contains(c.TipAbonamentId));
        var numarAbonamente = await db.Abonamente.CountAsync(a => tipIds.Contains(a.TipAbonamentId));

        return View(new CategorieStergeViewModel
        {
            Categorie = categorie,
            NumarComenzi = numarComenzi,
            NumarAbonamente = numarAbonamente
        });
    }

    [HttpPost("sterge/{id:int}")]
    [ActionName("Sterge")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StergeConfirmat(int id)
    {
        var categorie = await db.Categorii
            .Include(c => c.Tipuri)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categorie is null)
            return NotFoundView(id, "Categoria");

        var tipIds = categorie.Tipuri.Select(t => t.Id).ToList();
        var areIstoric = await db.Comenzi.AnyAsync(c => tipIds.Contains(c.TipAbonamentId))
                      || await db.Abonamente.AnyAsync(a => tipIds.Contains(a.TipAbonamentId));

        if (areIstoric)
        {
            TempData["MsgErr"] = $"Categoria „{categorie.Nume}” are comenzi sau abonamente emise în istoric și nu poate fi ștearsă fizic. Folosește opțiunea de dezactivare.";
            return RedirectToAction(nameof(Index));
        }

        var nume = categorie.Nume;
        try
        {
            if (categorie.Tipuri.Count > 0)
            {
                db.TipuriAbonament.RemoveRange(categorie.Tipuri);
            }
            db.Categorii.Remove(categorie);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["MsgErr"] = $"Eroare la ștergerea categoriei „{nume}” din baza de date.";
            return RedirectToAction(nameof(Index));
        }

        await LogAsync("Ștergere categorie", nume);
        TempData["MsgOk"] = $"Categoria „{nume}” a fost ștearsă din baza de date.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("tip/detalii/{id:int}")]
    public async Task<IActionResult> DetaliiTip(int id)
    {
        var tip = await db.TipuriAbonament
            .AsNoTracking()
            .Include(t => t.Categorie)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tip is null)
            return NotFoundView(id, "Tipul de abonament");

        var numarComenzi = await db.Comenzi.CountAsync(c => c.TipAbonamentId == id);
        var numarAbonamente = await db.Abonamente.CountAsync(a => a.TipAbonamentId == id);

        return View(new TipAbonamentDetaliiViewModel
        {
            Tip = tip,
            NumarComenzi = numarComenzi,
            NumarAbonamente = numarAbonamente
        });
    }

    [HttpGet("tip/editeaza/{id:int}")]
    public async Task<IActionResult> EditeazaTip(int id)
    {
        var tip = await db.TipuriAbonament
            .AsNoTracking()
            .Include(t => t.Categorie)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tip is null)
            return NotFoundView(id, "Tipul de abonament");

        return View(new TipAbonamentEditViewModel
        {
            Id = tip.Id,
            CategorieId = tip.CategorieId,
            CategorieNume = tip.Categorie.Nume,
            DurataLuni = tip.DurataLuni,
            Pret = tip.Pret,
            Activ = tip.Activ
        });
    }

    [HttpPost("tip/editeaza/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditeazaTip(int id, TipAbonamentEditViewModel formular)
    {
        var tip = await db.TipuriAbonament
            .Include(t => t.Categorie)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tip is null)
            return NotFoundView(id, "Tipul de abonament");

        formular.Id = tip.Id;
        formular.CategorieId = tip.CategorieId;
        formular.CategorieNume = tip.Categorie.Nume;

        if (ModelState.IsValid)
        {
            var existaAltTipCuAceeasiDurata = await db.TipuriAbonament
                .AnyAsync(t => t.CategorieId == tip.CategorieId && t.DurataLuni == formular.DurataLuni!.Value && t.Id != id);

            if (existaAltTipCuAceeasiDurata)
            {
                ModelState.AddModelError(nameof(formular.DurataLuni), MesajDuplicat(tip.Categorie.Nume, formular.DurataLuni!.Value));
            }
        }

        if (!ModelState.IsValid)
        {
            return View(formular);
        }

        tip.DurataLuni = formular.DurataLuni!.Value;
        tip.Pret = formular.Pret!.Value;
        tip.Activ = formular.Activ;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "A apărut o eroare la salvarea modificărilor în baza de date.");
            return View(formular);
        }

        var descriere = $"{tip.Categorie.Nume}, {Tarife.EticheteDurata(tip.DurataLuni)}, {tip.Pret} MDL";
        await LogAsync("Modificare tip abonament", descriere);

        TempData["MsgOk"] = $"Tipul de abonament ({descriere}) a fost actualizat.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("tip/sterge/{id:int}")]
    public async Task<IActionResult> StergeTip(int id)
    {
        var tip = await db.TipuriAbonament
            .AsNoTracking()
            .Include(t => t.Categorie)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tip is null)
            return NotFoundView(id, "Tipul de abonament");

        var numarComenzi = await db.Comenzi.CountAsync(c => c.TipAbonamentId == id);
        var numarAbonamente = await db.Abonamente.CountAsync(a => a.TipAbonamentId == id);

        return View(new TipAbonamentStergeViewModel
        {
            Tip = tip,
            NumarComenzi = numarComenzi,
            NumarAbonamente = numarAbonamente
        });
    }

    [HttpPost("tip/sterge/{id:int}")]
    [ActionName("StergeTip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StergeTipConfirmat(int id)
    {
        var tip = await db.TipuriAbonament
            .Include(t => t.Categorie)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tip is null)
            return NotFoundView(id, "Tipul de abonament");

        var areIstoric = await db.Comenzi.AnyAsync(c => c.TipAbonamentId == id)
                      || await db.Abonamente.AnyAsync(a => a.TipAbonamentId == id);

        var descriere = $"{tip.Categorie.Nume}, {Tarife.EticheteDurata(tip.DurataLuni)}";
        if (areIstoric)
        {
            TempData["MsgErr"] = $"Tipul de abonament ({descriere}) are comenzi sau abonamente emise și nu poate fi șters fizic. Folosește opțiunea de dezactivare.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            db.TipuriAbonament.Remove(tip);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["MsgErr"] = $"Eroare la ștergerea tipului de abonament ({descriere}).";
            return RedirectToAction(nameof(Index));
        }

        await LogAsync("Ștergere tip abonament", descriere);
        TempData["MsgOk"] = $"Tipul de abonament ({descriere}) a fost șters din baza de date.";
        return RedirectToAction(nameof(Index));
    }

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
        if (categorie is null) return NotFoundView(id, "Categoria");

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
        if (categorie is null) return NotFoundView(id, "Categoria");

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
        if (tip is null) return NotFoundView(id, "Tipul de abonament");

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
        if (tip is null) return NotFoundView(id, "Tipul de abonament");

        tip.Activ = !tip.Activ;
        await db.SaveChangesAsync();

        var descriere = $"{tip.Categorie.Nume}, {Tarife.EticheteDurata(tip.DurataLuni)}";
        await LogAsync(tip.Activ ? "Reactivare tip abonament" : "Dezactivare tip abonament", descriere);

        TempData["MsgOk"] = $"Tipul de abonament ({descriere}) a fost {(tip.Activ ? "reactivat" : "dezactivat")}.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult NotFoundView(int id, string tipEntitate)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NotFound", new CatalogNotFoundViewModel
        {
            Id = id,
            TipEntitate = tipEntitate
        });
    }

    private async Task<AdminCatalogViewModel> CreeazaModelAsync(string? cautare = null)
    {
        var toateCategoriile = await db.Categorii
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();

        var query = db.Categorii
            .AsNoTracking()
            .Include(c => c.Tipuri.OrderBy(t => t.DurataLuni))
            .AsQueryable();

        var termen = cautare?.Trim();
        if (!string.IsNullOrWhiteSpace(termen))
        {
            var termenLower = termen.ToLower();
            query = query.Where(c =>
                c.Nume.ToLower().Contains(termenLower) ||
                c.Descriere.ToLower().Contains(termenLower));
        }

        var categoriiFiltrate = await query.OrderBy(c => c.Id).ToListAsync();

        return new AdminCatalogViewModel
        {
            Cautare = termen,
            Categorii = categoriiFiltrate,
            ToateCategoriile = toateCategoriile
        };
    }

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
