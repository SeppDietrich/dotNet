using Microsoft.AspNetCore.Identity;

namespace TransAbon.Models;

/// <summary>Utilizatorul aplicației (obiectul „Utilizator” din caietul de sarcini).</summary>
public class ApplicationUser : IdentityUser
{
    public string Nume { get; set; } = "";
    public string Prenume { get; set; } = "";
    public bool Activ { get; set; } = true;
    public DateTime DataInregistrarii { get; set; } = DateTime.UtcNow;

    public string NumeComplet => $"{Prenume} {Nume}".Trim();
}
