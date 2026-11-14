using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Services;

public class AbonamentService(AppDbContext db, IJurnalService jurnalService) : IAbonamentService
{
    private static readonly char[] CaractereCod = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public async Task<string> GenereazaCodUnicAsync()
    {
        for (int incercare = 0; incercare < 30; incercare++)
        {
            var cod = $"TA-{RandomNumberGenerator.GetString(CaractereCod, 8)}";
            var exista = await db.Abonamente.AnyAsync(a => a.Cod == cod);
            if (!exista)
            {
                return cod;
            }
        }

        throw new InvalidOperationException("Eroare la generarea unui cod unic de abonament. Încearcă din nou.");
    }

    public DateOnly CalculeazaDataExpirare(DateOnly dataInceput, int durataLuni)
    {
        if (durataLuni <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durataLuni), "Durata trebuie să fie un număr pozitiv de luni.");
        }

        // Conform caietului de sarcini și README: DataExpirare = DataInceput + durata în luni − 1 zi
        return dataInceput.AddMonths(durataLuni).AddDays(-1);
    }

    public async Task<List<AbonamentClientItemViewModel>> ObtineAbonamenteClientAsync(string clientId)
    {
        var azi = DateOnly.FromDateTime(DateTime.UtcNow);

        var lista = await db.Abonamente
            .AsNoTracking()
            .Where(a => a.ClientId == clientId)
            .Include(a => a.TipAbonament)
            .ThenInclude(t => t.Categorie)
            .OrderByDescending(a => a.DataInceput)
            .ThenByDescending(a => a.Id)
            .ToListAsync();

        return lista.Select(a =>
        {
            StareAbonamentAfisata stareCalculata;
            if (a.Stare == StareAbonament.Anulat)
            {
                stareCalculata = StareAbonamentAfisata.Anulat;
            }
            else if (azi < a.DataInceput)
            {
                stareCalculata = StareAbonamentAfisata.Programat;
            }
            else if (a.Stare == StareAbonament.Expirat || azi > a.DataExpirare)
            {
                stareCalculata = StareAbonamentAfisata.Expirat;
            }
            else
            {
                stareCalculata = StareAbonamentAfisata.Activ;
            }

            var poateFiAnulat = a.Stare == StareAbonament.Activ && azi < a.DataInceput;

            return new AbonamentClientItemViewModel
            {
                Id = a.Id,
                Cod = a.Cod,
                Categorie = a.TipAbonament?.Categorie?.Nume ?? "Necunoscută",
                DurataLuni = a.TipAbonament?.DurataLuni ?? 1,
                Pret = a.TipAbonament?.Pret ?? 0,
                DataInceput = a.DataInceput,
                DataExpirare = a.DataExpirare,
                StareMemorata = a.Stare,
                StareCalculata = stareCalculata,
                PoateFiAnulat = poateFiAnulat
            };
        }).ToList();
    }

    public async Task<List<OptiuneTipAbonamentViewModel>> ObtineTipuriActiveAsync()
    {
        return await db.TipuriAbonament
            .AsNoTracking()
            .Include(t => t.Categorie)
            .Where(t => t.Activ && t.Categorie.Activ)
            .OrderBy(t => t.Categorie.Nume)
            .ThenBy(t => t.DurataLuni)
            .Select(t => new OptiuneTipAbonamentViewModel
            {
                Id = t.Id,
                CategorieId = t.CategorieId,
                CategorieNume = t.Categorie.Nume,
                DurataLuni = t.DurataLuni,
                Pret = t.Pret
            })
            .ToListAsync();
    }

    public async Task<List<CategorieOptiuneViewModel>> ObtineCategoriiActiveAsync()
    {
        return await db.Categorii
            .AsNoTracking()
            .Where(c => c.Activ && c.Tipuri.Any(t => t.Activ))
            .OrderBy(c => c.Nume)
            .Select(c => new CategorieOptiuneViewModel
            {
                Id = c.Id,
                Nume = c.Nume,
                Descriere = c.Descriere
            })
            .ToListAsync();
    }

    public async Task<(bool Succes, string? MesajEroare, Abonament? Abonament)> CreeazaAbonamentAsync(
        string clientId,
        int tipAbonamentId,
        int categorieId,
        int durataLuni,
        DateOnly dataInceput)
    {
        var azi = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dataInceput < azi)
        {
            return (false, "Data de început nu poate fi în trecut.", null);
        }

        TipAbonament? tip = null;
        if (tipAbonamentId > 0)
        {
            tip = await db.TipuriAbonament
                .Include(t => t.Categorie)
                .FirstOrDefaultAsync(t => t.Id == tipAbonamentId && t.Activ && t.Categorie.Activ);
        }

        if (tip is null && categorieId > 0 && durataLuni > 0)
        {
            tip = await db.TipuriAbonament
                .Include(t => t.Categorie)
                .FirstOrDefaultAsync(t => t.CategorieId == categorieId && t.DurataLuni == durataLuni && t.Activ && t.Categorie.Activ);
        }

        if (tip is null)
        {
            return (false, "Tipul de abonament selectat nu este valid sau nu mai este activ.", null);
        }

        var cod = await GenereazaCodUnicAsync();
        var dataExpirare = CalculeazaDataExpirare(dataInceput, tip.DurataLuni);

        var comanda = new Comanda
        {
            ClientId = clientId,
            TipAbonamentId = tip.Id,
            DataComenzii = DateTime.UtcNow,
            DataInceput = dataInceput,
            Stare = StareComanda.Confirmata
        };

        var abonament = new Abonament
        {
            ClientId = clientId,
            TipAbonamentId = tip.Id,
            Comanda = comanda,
            Cod = cod,
            DataInceput = dataInceput,
            DataExpirare = dataExpirare,
            Stare = StareAbonament.Activ
        };

        db.Comenzi.Add(comanda);
        db.Abonamente.Add(abonament);
        await db.SaveChangesAsync();

        var descriereJurnal = $"{abonament.Cod} ({tip.Categorie.Nume}, {tip.DurataLuni} luni, {tip.Pret} MDL)";
        await jurnalService.LogAsync(clientId, "Comandă", descriereJurnal);

        return (true, null, abonament);
    }

    public async Task<(bool Succes, string? MesajEroare, string? CodAbonament)> AnuleazaAbonamentAsync(
        string clientId,
        int abonamentId)
    {
        var abonament = await db.Abonamente
            .Include(a => a.Comanda)
            .FirstOrDefaultAsync(a => a.Id == abonamentId);

        if (abonament is null || abonament.ClientId != clientId)
        {
            return (false, "Abonamentul nu a fost găsit sau nu aparține contului tău.", null);
        }

        var azi = DateOnly.FromDateTime(DateTime.UtcNow);
        if (azi >= abonament.DataInceput || abonament.Stare != StareAbonament.Activ)
        {
            return (false, "Doar abonamentele viitoare (programate) pot fi anulate înainte de data de început.", null);
        }

        abonament.Stare = StareAbonament.Anulat;
        abonament.MotivAnulare = "Anulat de client";
        if (abonament.Comanda is not null)
        {
            abonament.Comanda.Stare = StareComanda.Anulata;
        }

        await db.SaveChangesAsync();
        await jurnalService.LogAsync(clientId, "Anulare", abonament.Cod);

        return (true, null, abonament.Cod);
    }
}
