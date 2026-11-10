namespace TransAbon.Models;

public class AdminRapoarteViewModel
{
    public DateOnly DeLa { get; set; }
    public DateOnly PanaLa { get; set; }
    public bool GeneratExplici { get; set; }

    public IReadOnlyList<RaportAbonamenteCategorieRow> AbonamentePeCategorie { get; set; } = [];
    public int TotalAbonamenteEmise { get; set; }
    public int ValoareTotalaLei { get; set; }

    public int TotalValidari { get; set; }
    public int ValidariValide { get; set; }
    public int ValidariInvalide { get; set; }
    public int ValidariExpirate { get; set; }
    public int ValidariAnulate { get; set; }
    public int ValidariCodInexistent { get; set; }
}

public class RaportAbonamenteCategorieRow
{
    public string Categorie { get; set; } = "";
    public int DurataLuni { get; set; }
    public int PretUnitar { get; set; }
    public int NumarAbonamente { get; set; }
    public int ValoareTotala => NumarAbonamente * PretUnitar;
}
