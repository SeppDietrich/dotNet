using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransAbon.Models;

namespace TransAbon.Data;

/// <summary>
/// Creează rolurile, conturile inițiale, catalogul de abonamente și, doar în Development, date demo.
/// Credențialele vin din configurare (SeedUsers:*:Email / Parola): appsettings.Development.json
/// în dezvoltare, user-secrets sau variabile de mediu în producție. Dacă lipsesc, contul nu se creează.
/// Seed-ul este idempotent: poate rula la fiecare pornire fără să dubleze datele.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration config)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<AppDbContext>();
        var env = sp.GetRequiredService<IHostEnvironment>();

        foreach (var role in AppRoles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUserAsync(users, config.GetSection("SeedUsers:Administrator"), "Admin", "Demo", AppRoles.Administrator, logger);
        var controlor = await EnsureUserAsync(users, config.GetSection("SeedUsers:Controlor"), "Controlor", "Demo", AppRoles.Controlor, logger);
        var client = await EnsureUserAsync(users, config.GetSection("SeedUsers:Client"), "Client", "Demo", AppRoles.Client, logger);

        await SeedCatalogAsync(db);

        if (env.IsDevelopment() && client is not null && controlor is not null)
        {
            await SeedDemoAsync(db, client, controlor, logger);
        }
    }

    /// <summary>Încarcă categoriile și tipurile de abonament din AbonamenteData, dacă tabelele sunt goale.</summary>
    private static async Task SeedCatalogAsync(AppDbContext db)
    {
        if (await db.Categorii.AnyAsync()) return;

        foreach (var sursa in AbonamenteData.Categorii)
        {
            var categorie = new Categorie { Nume = sursa.Nume, Descriere = sursa.Descriere, Activ = true };

            foreach (var luni in AbonamenteData.Perioade.Keys)
            {
                categorie.Tipuri.Add(new TipAbonament
                {
                    DurataLuni = luni,
                    // Preturi == null înseamnă categorie gratuită.
                    Pret = sursa.Preturi?[luni] ?? 0,
                    Activ = true
                });
            }

            db.Categorii.Add(categorie);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>Trei abonamente (expirat, activ, programat) și trei controale, ca în machetele existente.</summary>
    private static async Task SeedDemoAsync(AppDbContext db, ApplicationUser client, ApplicationUser controlor, ILogger logger)
    {
        if (await db.Abonamente.AnyAsync()) return;

        var tip = await db.TipuriAbonament
            .Include(t => t.Categorie)
            .FirstOrDefaultAsync(t => t.Categorie.Nume == "General" && t.DurataLuni == 1);
        if (tip is null)
        {
            logger.LogWarning("Datele demo nu au fost create: lipsește tipul General / 1 lună.");
            return;
        }

        var azi = DateOnly.FromDateTime(DateTime.UtcNow);

        var expirat = CreeazaAbonament(client, tip, azi.AddDays(-100), "TA-DEMO-0002", StareAbonament.Expirat);
        var activ = CreeazaAbonament(client, tip, azi.AddDays(-20), "TA-DEMO-0004", StareAbonament.Activ);
        var programat = CreeazaAbonament(client, tip, azi.AddDays(10), "TA-DEMO-0005", StareAbonament.Activ);
        db.Abonamente.AddRange(expirat, activ, programat);

        db.Controale.AddRange(
            new Control { Controlor = controlor, Abonament = activ, CodIntrodus = activ.Cod, DataOra = DateTime.UtcNow.AddMinutes(-30), Rezultat = RezultatControl.Valid },
            new Control { Controlor = controlor, Abonament = expirat, CodIntrodus = expirat.Cod, DataOra = DateTime.UtcNow.AddMinutes(-20), Rezultat = RezultatControl.Expirat },
            new Control { Controlor = controlor, Abonament = null, CodIntrodus = "TA-ZZZZZZZZ", DataOra = DateTime.UtcNow.AddMinutes(-10), Rezultat = RezultatControl.CodInexistent });

        await db.SaveChangesAsync();
    }

    private static Abonament CreeazaAbonament(ApplicationUser client, TipAbonament tip, DateOnly start, string cod, StareAbonament stare)
    {
        var comanda = new Comanda
        {
            Client = client,
            TipAbonament = tip,
            DataComenzii = DateTime.UtcNow,
            DataInceput = start,
            Stare = StareComanda.Confirmata
        };

        return new Abonament
        {
            Client = client,
            TipAbonament = tip,
            Comanda = comanda,
            Cod = cod,
            DataInceput = start,
            // Ex.: început 2026-09-23, 1 lună → expiră 2026-10-22.
            DataExpirare = start.AddMonths(tip.DurataLuni).AddDays(-1),
            Stare = stare
        };
    }

    private static async Task<ApplicationUser?> EnsureUserAsync(UserManager<ApplicationUser> users, IConfigurationSection section,
        string prenume, string nume, string role, ILogger logger)
    {
        var email = section["Email"];
        var parola = section["Parola"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(parola))
        {
            logger.LogWarning("Contul inițial pentru rolul {Role} nu a fost creat: lipsesc Email/Parola din configurare.", role);
            return null;
        }

        var existent = await users.FindByEmailAsync(email);
        if (existent is not null) return existent;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Prenume = prenume,
            Nume = nume,
            Activ = true,
            DataInregistrarii = DateTime.UtcNow,
            CodLegitimatie = role == AppRoles.Controlor ? "CTL-0001" : null
        };

        var result = await users.CreateAsync(user, parola);
        if (!result.Succeeded)
        {
            logger.LogError("Crearea contului pentru rolul {Role} a eșuat: {Errors}", role,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return null;
        }

        await users.AddToRoleAsync(user, role);
        return user;
    }
}
