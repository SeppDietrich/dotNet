namespace TransAbon.Models;

/// <summary>O verificare a unui abonament, efectuată de un controlor la bord.</summary>
public class Control
{
    public int Id { get; set; }

    public string ControlorId { get; set; } = "";
    public ApplicationUser Controlor { get; set; } = null!;

    /// <summary>Nul când codul introdus nu există (rezultat CodInexistent).</summary>
    public int? AbonamentId { get; set; }
    public Abonament? Abonament { get; set; }

    /// <summary>Codul exact introdus de controlor, păstrat și când nu există abonament.</summary>
    public string CodIntrodus { get; set; } = "";

    public DateTime DataOra { get; set; } = DateTime.UtcNow;

    public RezultatControl Rezultat { get; set; }

    /// <summary>Cursa selectată de controlor (opțional, cerința „R” din 5.3).</summary>
    public string? Cursa { get; set; }
}
