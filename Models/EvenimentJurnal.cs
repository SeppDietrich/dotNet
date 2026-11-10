namespace TransAbon.Models;

/// <summary>Înregistrare în jurnalul de evenimente: cine, când, ce a făcut și asupra cărui obiect.</summary>
public class EvenimentJurnal
{
    public int Id { get; set; }

    public DateTime DataOra { get; set; } = DateTime.UtcNow;

    /// <summary>Nul pentru evenimente fără utilizator (ex. încercare de autentificare eșuată).</summary>
    public string? UtilizatorId { get; set; }
    public ApplicationUser? Utilizator { get; set; }

    public string Actiune { get; set; } = "";
    public string Obiect { get; set; } = "";
}
