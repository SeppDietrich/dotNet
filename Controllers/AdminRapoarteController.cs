using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
[Route("admin/rapoarte")]
public class AdminRapoarteController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(DateOnly? deLa = null, DateOnly? panaLa = null)
    {
        var model = await ConstruiesteRaportAsync(deLa, panaLa);
        model.GeneratExplici = deLa.HasValue || panaLa.HasValue;
        return View(model);
    }

    [HttpGet("export-abonamente")]
    public async Task<IActionResult> ExportAbonamenteCsv(DateOnly? deLa = null, DateOnly? panaLa = null)
    {
        var raport = await ConstruiesteRaportAsync(deLa, panaLa);

        var sb = new StringBuilder();
        sb.AppendLine("Perioadă de la,Perioadă până la,Categorie,Durată (luni),Preț unitar (lei),Abonamente emise,Valoare totală (lei)");

        foreach (var r in raport.AbonamentePeCategorie)
        {
            sb.AppendLine(string.Join(",",
                CsvEscape(raport.DeLa.ToString("yyyy-MM-dd")),
                CsvEscape(raport.PanaLa.ToString("yyyy-MM-dd")),
                CsvEscape(r.Categorie),
                r.DurataLuni,
                r.PretUnitar,
                r.NumarAbonamente,
                r.ValoareTotala));
        }

        sb.AppendLine(string.Join(",",
            CsvEscape("TOTAL"),
            "",
            "",
            "",
            "",
            raport.TotalAbonamenteEmise,
            raport.ValoareTotalaLei));

        return CsvFile(sb.ToString(), $"raport-abonamente-{raport.DeLa:yyyyMMdd}-{raport.PanaLa:yyyyMMdd}.csv");
    }

    [HttpGet("export-validari")]
    public async Task<IActionResult> ExportValidariCsv(DateOnly? deLa = null, DateOnly? panaLa = null)
    {
        var raport = await ConstruiesteRaportAsync(deLa, panaLa);

        var sb = new StringBuilder();
        sb.AppendLine("Perioadă de la,Perioadă până la,Indicator,Număr validări");
        sb.AppendLine($"{raport.DeLa:yyyy-MM-dd},{raport.PanaLa:yyyy-MM-dd},{CsvEscape("Total validări")},{raport.TotalValidari}");
        sb.AppendLine($"{raport.DeLa:yyyy-MM-dd},{raport.PanaLa:yyyy-MM-dd},{CsvEscape("Validări valide")},{raport.ValidariValide}");
        sb.AppendLine($"{raport.DeLa:yyyy-MM-dd},{raport.PanaLa:yyyy-MM-dd},{CsvEscape("Validări invalide (total)")},{raport.ValidariInvalide}");
        sb.AppendLine($"{raport.DeLa:yyyy-MM-dd},{raport.PanaLa:yyyy-MM-dd},{CsvEscape("  - Expirate / în afara perioadei")},{raport.ValidariExpirate}");
        sb.AppendLine($"{raport.DeLa:yyyy-MM-dd},{raport.PanaLa:yyyy-MM-dd},{CsvEscape("  - Anulate")},{raport.ValidariAnulate}");
        sb.AppendLine($"{raport.DeLa:yyyy-MM-dd},{raport.PanaLa:yyyy-MM-dd},{CsvEscape("  - Cod inexistent")},{raport.ValidariCodInexistent}");

        return CsvFile(sb.ToString(), $"raport-validari-{raport.DeLa:yyyyMMdd}-{raport.PanaLa:yyyyMMdd}.csv");
    }

    private async Task<AdminRapoarteViewModel> ConstruiesteRaportAsync(DateOnly? deLa, DateOnly? panaLa)
    {
        var azi = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = deLa ?? azi.AddDays(-30);
        var end = panaLa ?? azi;

        if (end < start)
        {
            (start, end) = (end, start);
        }

        var startUtc = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endExclusiveUtc = end.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        // (a) Abonamentele emise în perioada selectată (după data comenzii), grupate în baza de date pe categorie și durată.
        var abonamenteGrupate = await db.Abonamente
            .AsNoTracking()
            .Where(a => a.Comanda.DataComenzii >= startUtc && a.Comanda.DataComenzii < endExclusiveUtc)
            .GroupBy(a => new
            {
                Categorie = a.TipAbonament.Categorie.Nume,
                a.TipAbonament.DurataLuni,
                a.TipAbonament.Pret
            })
            .Select(g => new RaportAbonamenteCategorieRow
            {
                Categorie = g.Key.Categorie,
                DurataLuni = g.Key.DurataLuni,
                PretUnitar = g.Key.Pret,
                NumarAbonamente = g.Count()
            })
            .OrderBy(r => r.Categorie)
            .ThenBy(r => r.DurataLuni)
            .ToListAsync();

        // (b) Validările din perioada selectată, grupate în baza de date după rezultat.
        var validariPeRezultat = await db.Controale
            .AsNoTracking()
            .Where(c => c.DataOra >= startUtc && c.DataOra < endExclusiveUtc)
            .GroupBy(c => c.Rezultat)
            .Select(g => new { Rezultat = g.Key, Numar = g.Count() })
            .ToListAsync();

        var valide = validariPeRezultat.FirstOrDefault(x => x.Rezultat == RezultatControl.Valid)?.Numar ?? 0;
        var expirate = validariPeRezultat.FirstOrDefault(x => x.Rezultat == RezultatControl.Expirat)?.Numar ?? 0;
        var anulate = validariPeRezultat.FirstOrDefault(x => x.Rezultat == RezultatControl.Anulat)?.Numar ?? 0;
        var inexistente = validariPeRezultat.FirstOrDefault(x => x.Rezultat == RezultatControl.CodInexistent)?.Numar ?? 0;
        var invalide = expirate + anulate + inexistente;

        return new AdminRapoarteViewModel
        {
            DeLa = start,
            PanaLa = end,
            AbonamentePeCategorie = abonamenteGrupate,
            TotalAbonamenteEmise = abonamenteGrupate.Sum(x => x.NumarAbonamente),
            ValoareTotalaLei = abonamenteGrupate.Sum(x => x.ValoareTotala),
            TotalValidari = valide + invalide,
            ValidariValide = valide,
            ValidariInvalide = invalide,
            ValidariExpirate = expirate,
            ValidariAnulate = anulate,
            ValidariCodInexistent = inexistente
        };
    }

    private FileContentResult CsvFile(string content, string fileName)
    {
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var preamble = encoding.GetPreamble();
        var body = encoding.GetBytes(content);
        var bytes = new byte[preamble.Length + body.Length];
        Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
        Buffer.BlockCopy(body, 0, bytes, preamble.Length, body.Length);

        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private static string CsvEscape(string? val)
    {
        if (string.IsNullOrEmpty(val)) return "";
        if (val.Contains(',') || val.Contains('"') || val.Contains('\n') || val.Contains('\r'))
        {
            return $"\"{val.Replace("\"", "\"\"")}\"";
        }
        return val;
    }
}
