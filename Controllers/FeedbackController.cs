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

        // Ca în PHP: deocamdată mesajul nu se salvează; doar se afișează confirmarea
        // și formularul se golește.
        ModelState.Clear();
        ViewBag.Trimis = true;
        return View(new FeedbackViewModel());
    }
}
