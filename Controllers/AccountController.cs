using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransAbon.Models;

namespace TransAbon.Controllers;

public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ILogger<AccountController> logger) : Controller
{
    private const string GenericLoginError = "E-mail sau parolă incorectă.";

    [HttpGet("autentificare")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(HomeFor(User.IsInRole(AppRoles.Administrator), User.IsInRole(AppRoles.Controlor)));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost("autentificare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            model.Password = "";
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null || !user.Activ)
        {
            logger.LogWarning("Autentificare eșuată pentru {Email}", model.Email);
            ModelState.AddModelError(string.Empty, GenericLoginError);
            model.Password = "";
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            var roles = await userManager.GetRolesAsync(user);
            return Redirect(HomeFor(roles.Contains(AppRoles.Administrator), roles.Contains(AppRoles.Controlor)));
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("Cont blocat temporar: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Contul este blocat temporar după prea multe încercări. Încearcă peste 15 minute.");
        }
        else
        {
            logger.LogWarning("Autentificare eșuată pentru {Email}", model.Email);
            ModelState.AddModelError(string.Empty, GenericLoginError);
        }

        model.Password = "";
        return View(model);
    }

    [HttpGet("inregistrare")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(HomeFor(User.IsInRole(AppRoles.Administrator), User.IsInRole(AppRoles.Controlor)));
        }

        return View(new RegisterViewModel());
    }

    [HttpPost("inregistrare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Password = model.ConfirmPassword = "";
            return View(model);
        }

        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Nume = model.Nume.Trim(),
            Prenume = model.Prenume.Trim(),
            Activ = true,
            DataInregistrarii = DateTime.UtcNow
        };

        var create = await userManager.CreateAsync(user, model.Password);
        if (create.Succeeded)
        {
            // Rolul nu se alege din formular: orice cont creat aici este Client.
            var role = await userManager.AddToRoleAsync(user, AppRoles.Client);
            if (role.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return Redirect("/cont");
            }

            await userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Contul nu a putut fi creat. Încearcă din nou.");
        }
        else
        {
            foreach (var error in create.Errors)
            {
                ModelState.AddModelError(string.Empty, Translate(error));
            }
        }

        model.Password = model.ConfirmPassword = "";
        return View(model);
    }

    [HttpPost("deconectare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("acces-interzis")]
    public IActionResult Denied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private static string HomeFor(bool isAdmin, bool isControlor) =>
        isAdmin ? "/admin" : isControlor ? "/validare" : "/cont";

    private static string Translate(IdentityError error) => error.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "Există deja un cont cu acest e-mail.",
        "InvalidEmail" or "InvalidUserName" => "Introdu o adresă de e-mail validă.",
        "PasswordTooShort" => "Parola trebuie să aibă minim 8 caractere.",
        "PasswordRequiresUpper" => "Parola trebuie să conțină cel puțin o literă mare.",
        "PasswordRequiresLower" => "Parola trebuie să conțină cel puțin o literă mică.",
        "PasswordRequiresDigit" => "Parola trebuie să conțină cel puțin o cifră.",
        _ => "Datele introduse nu sunt valide."
    };
}
