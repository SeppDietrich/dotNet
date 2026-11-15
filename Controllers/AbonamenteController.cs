using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Controllers;

public class AbonamenteController(AppDbContext db) : Controller
{
    [HttpGet("abonamente")]
    public async Task<IActionResult> Index()
    {
        var categorii = await db.Categorii
            .AsNoTracking()
            .Where(c => c.Activ && c.Tipuri.Any(t => t.Activ))
            .Include(c => c.Tipuri.Where(t => t.Activ).OrderBy(t => t.DurataLuni))
            .OrderBy(c => c.Id)
            .ToListAsync();

        return View(new AbonamenteViewModel
        {
            Categorii = categorii,
            Durate = categorii.SelectMany(c => c.Tipuri).Select(t => t.DurataLuni).Distinct().Order().ToList()
        });
    }
}
