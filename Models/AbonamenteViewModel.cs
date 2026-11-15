using System.Globalization;

namespace TransAbon.Models;

public class AbonamenteViewModel
{
    public List<int> Durate { get; set; } = new();
    public List<Categorie> Categorii { get; set; } = new();
}

public static class Tarife
{
    private static readonly NumberFormatInfo PretFormat = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalDigits = 0
    };

    public static string FormateazaPret(int pret) => pret.ToString("N0", PretFormat);

    public static string EticheteDurata(int luni) => luni == 1 ? "1 lună" : $"{luni} luni";
}
