namespace TransAbon.Models;

/// <summary>Categorie de călători (General, Elev, Student...). Grupează tipurile de abonament.</summary>
public class Categorie
{
    public int Id { get; set; }
    public string Nume { get; set; } = "";
    public string Descriere { get; set; } = "";
    public bool Activ { get; set; } = true;

    public ICollection<TipAbonament> Tipuri { get; set; } = new List<TipAbonament>();
}
