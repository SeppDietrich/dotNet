using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Client)]
[Route("cont/profil")]
public class ProfilController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJurnalService jurnalService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return View(new ProfilPaginaViewModel { Date = DinUtilizator(user) });
    }

    [HttpPost("date")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizeazaDate(
        [Bind(Prefix = "Date")] ProfilDateViewModel date)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        if (!ModelState.IsValid)
        {
            return View("Index", new ProfilPaginaViewModel { Date = date });
        }

        var email = date.Email.Trim();
        user.Nume = date.Nume.Trim();
        user.Prenume = date.Prenume.Trim();

        IdentityResult rezultat;
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            // E-mailul este și UserName (ca la înregistrare); SetEmailAsync salvează și Nume/Prenume.
            rezultat = await userManager.SetEmailAsync(user, email);
            if (rezultat.Succeeded)
            {
                rezultat = await userManager.SetUserNameAsync(user, email);
            }
        }
        else
        {
            rezultat = await userManager.UpdateAsync(user);
        }

        if (!rezultat.Succeeded)
        {
            foreach (var error in rezultat.Errors)
            {
                ModelState.AddModelError(string.Empty, Translate(error));
            }
            return View("Index", new ProfilPaginaViewModel { Date = date });
        }

        // Schimbarea resetează ștampila de securitate; reînnoim cookie-ul (și claim-ul nume_complet din navbar).
        await signInManager.RefreshSignInAsync(user);
        await jurnalService.LogAsync(user.Id, "Actualizare profil", user.Email ?? email);

        TempData["MsgOk"] = "Datele contului au fost actualizate.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("parola")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SchimbaParola(
        [Bind(Prefix = "Parola")] SchimbaParolaViewModel parola)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        if (!ModelState.IsValid)
        {
            return View("Index", new ProfilPaginaViewModel { Date = DinUtilizator(user) });
        }

        var rezultat = await userManager.ChangePasswordAsync(user, parola.ParolaCurenta, parola.ParolaNoua);
        if (!rezultat.Succeeded)
        {
            foreach (var error in rezultat.Errors)
            {
                ModelState.AddModelError(string.Empty, Translate(error));
            }
            return View("Index", new ProfilPaginaViewModel { Date = DinUtilizator(user) });
        }

        await signInManager.RefreshSignInAsync(user);
        await jurnalService.LogAsync(user.Id, "Schimbare parolă", user.Email ?? "");

        TempData["MsgOk"] = "Parola a fost schimbată.";
        return RedirectToAction(nameof(Index));
    }

    private static ProfilDateViewModel DinUtilizator(ApplicationUser user) => new()
    {
        Nume = user.Nume,
        Prenume = user.Prenume,
        Email = user.Email ?? ""
    };

    private static string Translate(IdentityError error) => error.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "Există deja un cont cu acest e-mail.",
        "InvalidEmail" or "InvalidUserName" => "Introdu o adresă de e-mail validă.",
        "PasswordMismatch" => "Parola curentă este incorectă.",
        "PasswordTooShort" => "Parola trebuie să aibă minim 8 caractere.",
        "PasswordRequiresUpper" => "Parola trebuie să conțină cel puțin o literă mare.",
        "PasswordRequiresLower" => "Parola trebuie să conțină cel puțin o literă mică.",
        "PasswordRequiresDigit" => "Parola trebuie să conțină cel puțin o cifră.",
        _ => "Datele introduse nu sunt valide."
    };
}