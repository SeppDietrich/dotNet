using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TransAbon.Models;

namespace TransAbon.Services;

/// <summary>
/// Echivalentul includes/auth.php. Cât timp Database:Enabled este false, autentificarea nu funcționează (demo).
/// Baza de date nu este aleasă încă; la conectare se completează doar blocul marcat TODO.
/// </summary>
public class AuthService : IAuthService
{
    private static readonly PasswordHasher<string> Hasher = new();

    private readonly DatabaseOptions _db;

    public AuthService(IOptions<DatabaseOptions> db) => _db = db.Value;

    public Task<AuthResult> AutentificaUtilizatorAsync(string email, string parola)
    {
        if (!_db.Enabled)
        {
            return Task.FromResult(new AuthResult(false, null,
                "Autentificarea nu este încă activă: baza de date nu este conectată."));
        }

        // TODO (la conectarea bazei de date):
        //  1. Se caută în tabelul "utilizatori" (id, email, parola_hash, rol, activ)
        //     utilizatorul cu e-mailul dat și activ = 1.
        //  2. Dacă există și VerificaParola(parola_hash, parola) este true:
        //         return new AuthResult(true, new AuthUser(id, rol), null);
        //  3. Altfel: "E-mail sau parolă incorectă."
        //  4. La excepții de conexiune: "Serviciul de autentificare nu este disponibil momentan."
        return Task.FromResult(new AuthResult(false, null,
            "Serviciul de autentificare nu este disponibil momentan."));
    }

    /// <summary>Verifică o parolă față de hash-ul salvat (echivalentul password_verify din PHP).</summary>
    // Atenție: hash-urile create cu password_hash din PHP nu sunt compatibile cu PasswordHasher.
    internal static bool VerificaParola(string email, string parolaHash, string parola)
    {
        var rezultat = Hasher.VerifyHashedPassword(email, parolaHash, parola);
        return rezultat != PasswordVerificationResult.Failed;
    }
}
