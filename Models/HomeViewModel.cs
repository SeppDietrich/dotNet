namespace TransAbon.Models;

public class HomeViewModel
{
    public string DenumireProiect { get; set; } = "";
    public string DescriereProiect { get; set; } = "";

    /// <summary>Sursa imaginii curente din carusel; null dacă folderul carousel-images este gol.</summary>
    public string? CarouselSrc { get; set; }

    public IReadOnlyList<CategorieAbonament> Categorii { get; set; } = Array.Empty<CategorieAbonament>();
}
