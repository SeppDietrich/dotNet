namespace TransAbon.Models;

/// <summary>Catalogul afișat public: doar categoriile și tipurile active.</summary>
public class CatalogPublicViewModel
{
    /// <summary>Duratele (în luni) care apar în cel puțin un tip activ, crescător.</summary>
    public IReadOnlyList<int> Perioade { get; set; } = [];

    public IReadOnlyList<CategoriePublicItem> Categorii { get; set; } = [];
}

public class CategoriePublicItem
{
    public int Id { get; set; }
    public string Nume { get; set; } = "";
    public string Descriere { get; set; } = "";

    /// <summary>Cheia este durata în luni, valoarea este prețul în lei.</summary>
    public IReadOnlyDictionary<int, int> Preturi { get; set; } = new Dictionary<int, int>();

    public bool EsteGratuit => Preturi.Count > 0 && Preturi.Values.All(p => p == 0);
    public int PretMinim => Preturi.Count > 0 ? Preturi.Values.Min() : 0;
}