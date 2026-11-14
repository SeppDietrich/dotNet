using Microsoft.AspNetCore.Mvc;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Controllers;

public class FeedbackController(AppDbContext db) : Controller
{
    [HttpGet("feedback")]
    public IActionResult Index() => View(new FeedbackViewModel());

    [HttpPost("feedback")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(FeedbackViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        db.Feedbackuri.Add(new Feedback
        {
            Nume = model.Nume.Trim(),
            Email = model.Email.Trim(),
            Evaluare = model.Evaluare!.Value,
            Mesaj = model.Mesaj.Trim(),
            DataTrimiterii = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        // După salvare se afișează confirmarea, iar formularul se golește.
        ModelState.Clear();
        ViewBag.Trimis = true;
        return View(new FeedbackViewModel());
    }
}