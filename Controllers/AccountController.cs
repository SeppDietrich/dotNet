using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _auth;

    public AccountController(IAuthService auth) => _auth = auth;

    [HttpGet("autentificare")]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost("autentificare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var rezultat = await _auth.AutentificaUtilizatorAsync(model.Email.Trim(), model.Password);

        if (rezultat.Ok && rezultat.User is not null)
        {
            // Echivalentul session_regenerate_id din PHP: se golește sesiunea veche.
            // Notă: ASP.NET Core nu poate schimba identificatorul sesiunii în aceeași cerere;
            // la conectarea bazei de date se recomandă autentificare prin cookie.
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("user_id", rezultat.User.Id);
            HttpContext.Session.SetString("user_rol", rezultat.User.Rol);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, rezultat.Eroare ?? "Autentificarea a eșuat.");
        model.Password = "";
        return View(model);
    }
}
