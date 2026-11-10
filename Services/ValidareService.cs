using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Services;

public class ValidareService(AppDbContext db, IJurnalService jurnalService) : IValidareService
{
    public async Task<ValidareRezultatItem> VerificaCodAsync(string controlorId, string codIntrodus)
    {
        var codNormalizat = (codIntrodus ?? "").Trim().ToUpperInvariant();
        if (codNormalizat.Length > 64)
        {
            codNormalizat = codNormalizat[..64];
        }

        var abonament = await db.Abonamente
            .Include(a => a.TipAbonament)
            .ThenInclude(t => t.Categorie)
            .FirstOrDefaultAsync(a => a.Cod.ToUpper() == codNormalizat);

        var azi = DateOnly.FromDateTime(DateTime.UtcNow);
        RezultatControl rezultat;
        bool esteProgramat = false;
        string titlu;
        string detalii;

        if (abonament is null)
        {
            rezultat = RezultatControl.CodInexistent;
            titlu = "COD INEXISTENT";
            detalii = $"Nu există niciun abonament cu codul „{codNormalizat}”.";
        }
        else if (abonament.Stare == StareAbonament.Anulat)
        {
            rezultat = RezultatControl.Anulat;
            titlu = "ANULAT";
            detalii = $"Abonamentul {abonament.Cod} a fost anulat.";
        }
        else if (abonament.Stare == StareAbonament.Expirat || azi > abonament.DataExpirare)
        {
            rezultat = RezultatControl.Expirat;
            titlu = "EXPIRAT";
            detalii = $"Abonamentul a expirat la {abonament.DataExpirare:yyyy-MM-dd}.";
        }
        else if (azi < abonament.DataInceput)
        {
            // Abonament care nu a început încă (programat): nu este valabil azi.
            // În tabela Controale se înregistrează cu RezultatControl.Expirat (în afara perioadei de valabilitate),
            // iar în interfață se afișează explicit ca PROGRAMAT (NEÎNCEPUT).
            rezultat = RezultatControl.Expirat;
            esteProgramat = true;
            titlu = "PROGRAMAT (NEÎNCEPUT)";
            detalii = $"Abonamentul nu a început încă. Valabil în perioada {abonament.DataInceput:yyyy-MM-dd} – {abonament.DataExpirare:yyyy-MM-dd}.";
        }
        else
        {
            rezultat = RezultatControl.Valid;
            titlu = "VALID";
            var infoCategorie = abonament.TipAbonament?.Categorie?.Nume is { Length: > 0 } cat
                ? $" ({cat}, {abonament.TipAbonament.DurataLuni} {(abonament.TipAbonament.DurataLuni == 1 ? "lună" : "luni")})"
                : "";
            detalii = $"Valabil în perioada {abonament.DataInceput:yyyy-MM-dd} – {abonament.DataExpirare:yyyy-MM-dd}{infoCategorie}.";
        }

        var control = new Control
        {
            ControlorId = controlorId,
            AbonamentId = abonament?.Id,
            CodIntrodus = codNormalizat,
            DataOra = DateTime.UtcNow,
            Rezultat = rezultat
        };

        db.Controale.Add(control);
        await db.SaveChangesAsync();

        await jurnalService.LogAsync(controlorId, "Validare", $"{codNormalizat} ({titlu})");

        return new ValidareRezultatItem
        {
            CodIntrodus = codNormalizat,
            Rezultat = rezultat,
            EsteProgramat = esteProgramat,
            TitluAfisat = titlu,
            Detalii = detalii
        };
    }

    public async Task<IReadOnlyList<ValidareIstoricItem>> ObtineValidariAziAsync(string controlorId)
    {
        var inceputZi = DateTime.UtcNow.Date;
        var sfarsitZi = inceputZi.AddDays(1);
        var azi = DateOnly.FromDateTime(inceputZi);

        var controale = await db.Controale
            .AsNoTracking()
            .Include(c => c.Abonament)
            .Where(c => c.ControlorId == controlorId && c.DataOra >= inceputZi && c.DataOra < sfarsitZi)
            .OrderByDescending(c => c.DataOra)
            .ToListAsync();

        return controale.Select(c =>
        {
            bool programat = c.Abonament is not null
                             && c.Abonament.Stare == StareAbonament.Activ
                             && DateOnly.FromDateTime(c.DataOra) < c.Abonament.DataInceput;

            var (eticheta, clasaBadge) = programat
                ? ("PROGRAMAT (NEÎNCEPUT)", "badge-programat")
                : c.Rezultat switch
                {
                    RezultatControl.Valid => ("VALID", "badge-valid"),
                    RezultatControl.Expirat => ("EXPIRAT", "badge-expirat"),
                    RezultatControl.Anulat => ("ANULAT", "badge-anulat"),
                    _ => ("COD INEXISTENT", "badge-inexistent")
                };

            return new ValidareIstoricItem
            {
                DataOra = c.DataOra,
                CodIntrodus = c.CodIntrodus,
                Rezultat = c.Rezultat,
                EsteProgramat = programat,
                EtichetaRezultat = eticheta,
                ClasaBadge = clasaBadge
            };
        }).ToList();
    }
}
