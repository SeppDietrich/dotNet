using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;
using TransAbon.Services;

namespace TransAbon.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
[Route("admin/utilizatori")]
public class AdminUtilizatoriController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IJurnalService jurnalService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? cautare = null)
    {
        var model = new AdminUtilizatoriViewModel
        {
            Cautare = cautare?.Trim(),
            Utilizatori = await ObtineUtilizatoriAsync(cautare)
        };

        return View(model);
    }

    [HttpPost("controlor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreeazaControlor(
        [Bind(Prefix = "FormularControlor")] CreareControlorViewModel formular,
        string? cautare = null)
    {
        if (!ModelState.IsValid)
        {
            formular.Password = "";
            return View("Index", new AdminUtilizatoriViewModel
            {
                Cautare = cautare?.Trim(),
                Utilizatori = await ObtineUtilizatoriAsync(cautare),
                FormularControlor = formular
            });
        }

        var email = formular.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Nume = formular.Nume.Trim(),
            Prenume = formular.Prenume.Trim(),
            CodLegitimatie = formular.CodLegitimatie.Trim(),
            Activ = true,
            DataInregistrarii = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, formular.Password);
        if (createResult.Succeeded)
        {
            // Rolul se stabilește exclusiv în cod: conturile create aici sunt Controlor.
            var roleResult = await userManager.AddToRoleAsync(user, AppRoles.Controlor);
            if (roleResult.Succeeded)
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await jurnalService.LogAsync(adminId, "Creare cont", $"{user.Email} ({AppRoles.Controlor}, legitimație: {user.CodLegitimatie})");

                TempData["MsgOk"] = $"Contul de controlor pentru {user.Email} a fost creat.";
                return RedirectToAction(nameof(Index), new { cautare });
            }

            await userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Rolul de controlor nu a putut fi atribuit. Încearcă din nou.");
        }
        else
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, Translate(error));
            }
        }

        formular.Password = "";
        return View("Index", new AdminUtilizatoriViewModel
        {
            Cautare = cautare?.Trim(),
            Utilizatori = await ObtineUtilizatoriAsync(cautare),
            FormularControlor = formular
        });
    }

    [HttpPost("stare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ComutaStare(string id, string? cautare = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index), new { cautare });
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["MsgErr"] = "Utilizatorul nu a fost găsit.";
            return RedirectToAction(nameof(Index), new { cautare });
        }

        var adminCurentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user.Activ)
        {
            if (user.Id == adminCurentId)
            {
                TempData["MsgErr"] = "Nu îți poți dezactiva propriul cont.";
                return RedirectToAction(nameof(Index), new { cautare });
            }

            var esteAdmin = await userManager.IsInRoleAsync(user, AppRoles.Administrator);
            if (esteAdmin)
            {
                var adminiActivi = await (
                    from u in db.Users
                    join ur in db.UserRoles on u.Id equals ur.UserId
                    join r in db.Roles on ur.RoleId equals r.Id
                    where r.Name == AppRoles.Administrator && u.Activ
                    select u.Id)
                    .CountAsync();

                if (adminiActivi <= 1)
                {
                    TempData["MsgErr"] = "Ultimul administrator activ nu poate fi dezactivat.";
                    return RedirectToAction(nameof(Index), new { cautare });
                }
            }
        }

        user.Activ = !user.Activ;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["MsgErr"] = "Starea contului nu a putut fi modificată.";
            return RedirectToAction(nameof(Index), new { cautare });
        }

        if (!user.Activ)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        var roluri = await userManager.GetRolesAsync(user);
        var rolAfisat = roluri.FirstOrDefault() ?? "Fără rol";
        var actiuneJurnal = user.Activ ? "Reactivare cont" : "Dezactivare cont";
        await jurnalService.LogAsync(adminCurentId, actiuneJurnal, $"{user.Email} ({rolAfisat})");

        TempData["MsgOk"] = user.Activ
            ? $"Contul {user.Email} a fost reactivat."
            : $"Contul {user.Email} a fost dezactivat.";

        return RedirectToAction(nameof(Index), new { cautare });
    }

    private async Task<IReadOnlyList<UtilizatorItemViewModel>> ObtineUtilizatoriAsync(string? cautare)
    {
        var query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(cautare))
        {
            var termen = cautare.Trim().ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(termen)) ||
                u.Nume.ToLower().Contains(termen) ||
                u.Prenume.ToLower().Contains(termen) ||
                (u.Prenume + " " + u.Nume).ToLower().Contains(termen) ||
                (u.Nume + " " + u.Prenume).ToLower().Contains(termen));
        }

        return await (
            from u in query
            join ur in db.UserRoles on u.Id equals ur.UserId into urGroup
            from ur in urGroup.DefaultIfEmpty()
            join r in db.Roles on ur.RoleId equals r.Id into rGroup
            from r in rGroup.DefaultIfEmpty()
            orderby u.DataInregistrarii descending
            select new UtilizatorItemViewModel
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "",
                NumeComplet = (u.Prenume + " " + u.Nume).Trim(),
                Rol = r != null ? (r.Name ?? "") : "",
                CodLegitimatie = u.CodLegitimatie,
                Activ = u.Activ,
                DataInregistrarii = u.DataInregistrarii
            })
            .ToListAsync();
    }

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
