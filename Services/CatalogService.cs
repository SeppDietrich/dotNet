using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Services;

public class CatalogService(AppDbContext db) : ICatalogService
{
    public async Task<CatalogPublicViewModel> GetPublicAsync()
    {
        var categorii = await db.Categorii
            .AsNoTracking()
            .Where(c => c.Activ)
            .OrderBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.Nume,
                c.Descriere,
                Tipuri = c.Tipuri
                    .Where(t => t.Activ)
                    .Select(t => new { t.DurataLuni, t.Pret })
                    .ToList()
            })
            .ToListAsync();

        var items = categorii
            .Where(c => c.Tipuri.Count > 0)
            .Select(c => new CategoriePublicItem
            {
                Id = c.Id,
                Nume = c.Nume,
                Descriere = c.Descriere,
                Preturi = c.Tipuri
                    .OrderBy(t => t.DurataLuni)
                    .ToDictionary(t => t.DurataLuni, t => t.Pret)
            })
            .ToList();

        return new CatalogPublicViewModel
        {
            Perioade = items.SelectMany(i => i.Preturi.Keys).Distinct().Order().ToList(),
            Categorii = items
        };
    }
}