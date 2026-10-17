namespace TransAbon.Services;

public record AuthUser(int Id, string Rol);

/// <summary>Echivalentul array-ului ['ok', 'user', 'eroare'] din includes/auth.php.</summary>
public record AuthResult(bool Ok, AuthUser? User, string? Eroare);

public interface IAuthService
{
    Task<AuthResult> AutentificaUtilizatorAsync(string email, string parola);
}
