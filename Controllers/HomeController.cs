using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

public class HomeController : Controller
{
    private const string CurrentImageKey = "current_image";

    private readonly ICarouselService _carousel;
    private readonly AppDbContext _db;

    public HomeController(ICarouselService carousel, AppDbContext db)
    {
        _carousel = carousel;
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var current = HttpContext.Session.GetInt32(CurrentImageKey) ?? 0;

        var model = new HomeViewModel
        {
            DenumireProiect = "TransAbon – Sistemul de gestionare a abonamentelor de transport",
            DescriereProiect = "Aplicație Web prin care clienții comandă online abonamente de transport, " +
                               "iar controlorii verifică rapid valabilitatea lor prin codul unic al abonamentului.",
            CarouselSrc = _carousel.GetImageSrc(current),
            Categorii = await _db.Categorii
                .AsNoTracking()
                .Where(c => c.Activ && c.Tipuri.Any(t => t.Activ))
                .Include(c => c.Tipuri.Where(t => t.Activ).OrderBy(t => t.DurataLuni))
                .OrderBy(c => c.Id)
                .ToListAsync()
        };
        return View(model);
    }

    [HttpGet("despre")]
    public IActionResult About() => View();

    [HttpGet("contact")]
    public IActionResult Contact() => View();

    [HttpPost("carusel/urmator")]
    [ValidateAntiForgeryToken]
    public IActionResult Next()
    {
        var current = HttpContext.Session.GetInt32(CurrentImageKey) ?? 0;
        HttpContext.Session.SetInt32(CurrentImageKey, current + 1);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("carusel/anterior")]
    [ValidateAntiForgeryToken]
    public IActionResult Previous()
    {
        var current = HttpContext.Session.GetInt32(CurrentImageKey) ?? 0;
        HttpContext.Session.SetInt32(CurrentImageKey, current - 1);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("eroare")]
    [HttpPost("eroare")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
