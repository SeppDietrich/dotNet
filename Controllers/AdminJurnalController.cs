using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
[Route("admin/jurnal")]
public class AdminJurnalController(AppDbContext db) : Controller
{
    private const int PageSize = 20;

    private static readonly string[] ActiuniStandard =
    [
        "Autentificare",
        "Autentificare eșuată",
        "Creare cont",
        "Dezactivare cont",
        "Reactivare cont",
        "Comandă",
        "Validare",
        "Anulare"
    ];

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? utilizator = null,
        string? actiune = null,
        DateOnly? deLa = null,
        DateOnly? panaLa = null,
        int pagina = 1)
    {
        var query = db.Jurnal.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(utilizator))
        {
            var termen = utilizator.Trim().ToLower();
            query = query.Where(j =>
                (j.Utilizator != null && (
                    (j.Utilizator.Email != null && j.Utilizator.Email.ToLower().Contains(termen)) ||
                    j.Utilizator.Nume.ToLower().Contains(termen) ||
                    j.Utilizator.Prenume.ToLower().Contains(termen) ||
                    (j.Utilizator.Prenume + " " + j.Utilizator.Nume).ToLower().Contains(termen))) ||
                j.Obiect.ToLower().Contains(termen));
        }

        if (!string.IsNullOrWhiteSpace(actiune))
        {
            var actiuneTrim = actiune.Trim();
            query = query.Where(j => j.Actiune == actiuneTrim);
        }

        if (deLa.HasValue)
        {
            var startUtc = deLa.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(j => j.DataOra >= startUtc);
        }

        if (panaLa.HasValue)
        {
            var endExclusiveUtc = panaLa.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(j => j.DataOra < endExclusiveUtc);
        }

        var totalEvenimente = await query.CountAsync();
        var totalPagini = Math.Max(1, (int)Math.Ceiling(totalEvenimente / (double)PageSize));
        pagina = Math.Clamp(pagina, 1, totalPagini);

        var evenimente = await query
            .OrderByDescending(j => j.DataOra)
            .ThenByDescending(j => j.Id)
            .Skip((pagina - 1) * PageSize)
            .Take(PageSize)
            .Select(j => new EvenimentJurnalItemViewModel
            {
                Id = j.Id,
                DataOra = j.DataOra,
                UtilizatorAfisat = j.Utilizator != null
                    ? (j.Utilizator.Email ?? j.Utilizator.UserName ?? "Utilizator")
                    : "Neautentificat / Sistem",
                Actiune = j.Actiune,
                Obiect = j.Obiect
            })
            .ToListAsync();

        var actiuniDinDb = await db.Jurnal
            .AsNoTracking()
            .Select(j => j.Actiune)
            .Distinct()
            .ToListAsync();

        var actiuniDisponibile = ActiuniStandard
            .Union(actiuniDinDb)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .OrderBy(a => a)
            .ToList();

        return View(new AdminJurnalViewModel
        {
            Utilizator = utilizator?.Trim(),
            Actiune = actiune?.Trim(),
            DeLa = deLa,
            PanaLa = panaLa,
            PaginaCurenta = pagina,
            TotalPagini = totalPagini,
            TotalEvenimente = totalEvenimente,
            ActiuniDisponibile = actiuniDisponibile,
            Evenimente = evenimente
        });
    }
}
