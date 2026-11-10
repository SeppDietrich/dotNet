using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;

namespace TransAbon.Controllers;

public class FeedbackController : Controller
{
    [HttpGet("feedback")]
    public IActionResult Index() => View(new FeedbackViewModel());

    [HttpPost("feedback")]
    [ValidateAntiForgeryToken]
    public IActionResult Index(FeedbackViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // TODO (sarcina 1): salvează mesajul în baza de date (entitatea Feedback).
        // Deocamdată se afișează doar confirmarea și formularul se golește.
        ModelState.Clear();
        ViewBag.Trimis = true;
        return View(new FeedbackViewModel());
    }
}
