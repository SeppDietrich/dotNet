using Microsoft.AspNetCore.Identity;
using TransAbon.Models;

namespace TransAbon.Data;

/// <summary>
/// Creează rolurile și conturile inițiale. Credențialele vin din configurare
/// (SeedUsers:*:Email / Parola): appsettings.Development.json în dezvoltare,
/// user-secrets sau variabile de mediu în producție. Dacă lipsesc, contul nu se creează.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration config)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in AppRoles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));
            }
        }

        await EnsureUserAsync(users, config.GetSection("SeedUsers:Administrator"), "Admin", "Demo", AppRoles.Administrator, logger);
        await EnsureUserAsync(users, config.GetSection("SeedUsers:Controlor"), "Controlor", "Demo", AppRoles.Controlor, logger);
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> users, IConfigurationSection section,
        string prenume, string nume, string role, ILogger logger)
    {
        var email = section["Email"];
        var parola = section["Parola"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(parola))
        {
            logger.LogWarning("Contul inițial pentru rolul {Role} nu a fost creat: lipsesc Email/Parola din configurare.", role);
            return;
        }

        if (await users.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Prenume = prenume,
            Nume = nume,
            Activ = true,
            DataInregistrarii = DateTime.UtcNow
        };

        var result = await users.CreateAsync(user, parola);
        if (!result.Succeeded)
        {
            logger.LogError("Crearea contului pentru rolul {Role} a eșuat: {Errors}", role,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await users.AddToRoleAsync(user, role);
    }
}
