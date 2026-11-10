namespace TransAbon.Models;

/// <summary>Solicitarea online a unui client pentru un abonament.</summary>
public class Comanda
{
    public int Id { get; set; }

    public string ClientId { get; set; } = "";
    public ApplicationUser Client { get; set; } = null!;

    public int TipAbonamentId { get; set; }
    public TipAbonament TipAbonament { get; set; } = null!;

    public DateTime DataComenzii { get; set; } = DateTime.UtcNow;

    /// <summary>Data de la care abonamentul devine valabil, aleasă de client.</summary>
    public DateOnly DataInceput { get; set; }

    public StareComanda Stare { get; set; } = StareComanda.Confirmata;

    public Abonament? Abonament { get; set; }
}
