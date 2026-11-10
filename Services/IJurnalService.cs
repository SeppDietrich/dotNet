namespace TransAbon.Services;

/// <summary>Serviciu pentru înregistrarea acțiunilor în jurnalul de audit.</summary>
public interface IJurnalService
{
    /// <summary>
    /// Salvează un eveniment în jurnalul de audit.
    /// </summary>
    /// <param name="utilizatorId">ID-ul utilizatorului (sau null pentru acțiuni neautentificate, ex. login eșuat).</param>
    /// <param name="actiune">Denumirea scurtă a acțiunii (ex. Autentificare, Autentificare eșuată, Creare cont, Comandă, Validare, Anulare).</param>
    /// <param name="obiect">Detalii despre obiectul acțiunii (ex. cod abonament, e-mail, categorie).</param>
    Task LogAsync(string? utilizatorId, string actiune, string obiect = "");
}
