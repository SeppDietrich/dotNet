namespace TransAbon.Models;

/// <summary>Dreptul electronic de a călători, identificat printr-un cod unic.</summary>
public class Abonament
{
    public int Id { get; set; }

    public string ClientId { get; set; } = "";
    public ApplicationUser Client { get; set; } = null!;

    public int ComandaId { get; set; }
    public Comanda Comanda { get; set; } = null!;

    public int TipAbonamentId { get; set; }
    public TipAbonament TipAbonament { get; set; } = null!;

    /// <summary>Cod unic, greu de ghicit (ex. TA-8F3K29QD). Se generează în aplicație.</summary>
    public string Cod { get; set; } = "";

    public DateOnly DataInceput { get; set; }
    public DateOnly DataExpirare { get; set; }

    public StareAbonament Stare { get; set; } = StareAbonament.Activ;

    /// <summary>Obligatoriu când administratorul anulează manual un abonament (caiet 5.4).</summary>
    public string? MotivAnulare { get; set; }
}
