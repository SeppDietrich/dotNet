using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
[Route("admin/feedback")]
public class AdminFeedbackController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var mesaje = await db.Feedbackuri
            .AsNoTracking()
            .OrderByDescending(f => f.DataTrimiterii)
            .ThenByDescending(f => f.Id)
            .ToListAsync();

        return View(new AdminFeedbackViewModel
        {
            TotalMesaje = mesaje.Count,
            MediaEvaluarilor = mesaje.Count > 0 ? mesaje.Average(f => f.Evaluare) : null,
            Mesaje = mesaje
        });
    }
}