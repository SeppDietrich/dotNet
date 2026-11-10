namespace TransAbon.Models;

/// <summary>Șablon comercial: categorie + durată + preț. Pe baza lui se emit abonamentele.</summary>
public class TipAbonament
{
    public int Id { get; set; }

    public int CategorieId { get; set; }
    public Categorie Categorie { get; set; } = null!;

    /// <summary>Durata de valabilitate, în luni (1, 3, 6).</summary>
    public int DurataLuni { get; set; }

    /// <summary>Prețul în lei (MDL). 0 înseamnă abonament gratuit.</summary>
    public int Pret { get; set; }

    public bool Activ { get; set; } = true;

    public bool EsteGratuit => Pret == 0;
}
