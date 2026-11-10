using System.Globalization;

namespace TransAbon.Models;

/// <summary>O categorie de abonament. Preturi == null înseamnă abonament gratuit.</summary>
public record CategorieAbonament(
    string Nume,
    string Descriere,
    IReadOnlyDictionary<int, int>? Preturi)
{
    public bool EsteGratuit => Preturi is null;
}

/// <summary>
/// Tarifele (MDL) într-un singur loc. Cheia perioadei este numărul de luni.
/// Servește paginile publice și încarcă tabelele Categorii/TipuriAbonament la prima pornire (DbSeeder).
/// TODO (sarcina 4): paginile trec pe baza de date, iar această clasă se elimină.
/// </summary>
public static class AbonamenteData
{
    public static IReadOnlyDictionary<int, string> Perioade { get; } = new Dictionary<int, string>
    {
        [1] = "1 lună",
        [3] = "3 luni",
        [6] = "6 luni",
    };

    public static IReadOnlyList<CategorieAbonament> Categorii { get; } = new List<CategorieAbonament>
    {
        new("General", "Pentru toți călătorii.",
            new Dictionary<int, int> { [1] = 273, [3] = 693, [6] = 1134 }),
        new("Elev", "Pentru elevi.",
            new Dictionary<int, int> { [1] = 136, [3] = 346, [6] = 567 }),
        new("Student / angajat în educație și sănătate", "Pentru studenți, personal didactic și medical.",
            new Dictionary<int, int> { [1] = 191, [3] = 485, [6] = 794 }),
        new("Elevi clasele I–IV", "Abonament gratuit pentru elevii din clasele primare.", null),
    };

    private static readonly NumberFormatInfo PretFormat = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalDigits = 0
    };

    /// <summary>Formatează prețul cu punct ca separator de mii: 1134 → "1.134".</summary>
    public static string FormateazaPret(int pret) => pret.ToString("N0", PretFormat);
}
