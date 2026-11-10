using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Services;

/// <summary>Implementarea serviciului de jurnalizare în baza de date (tabelul Jurnal).</summary>
public class JurnalService(AppDbContext db) : IJurnalService
{
    public async Task LogAsync(string? utilizatorId, string actiune, string obiect = "")
    {
        var actiuneCurata = (actiune ?? "").Trim();
        if (actiuneCurata.Length > 100)
        {
            actiuneCurata = actiuneCurata[..100];
        }

        var obiectCurat = (obiect ?? "").Trim();
        if (obiectCurat.Length > 300)
        {
            obiectCurat = obiectCurat[..300];
        }

        db.Jurnal.Add(new EvenimentJurnal
        {
            DataOra = DateTime.UtcNow,
            UtilizatorId = string.IsNullOrWhiteSpace(utilizatorId) ? null : utilizatorId,
            Actiune = actiuneCurata,
            Obiect = obiectCurat
        });

        await db.SaveChangesAsync();
    }
}
