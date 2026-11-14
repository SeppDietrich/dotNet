using System.Globalization;

namespace TransAbon.Models;

/// <summary>Formatări comune pentru prețuri și perioade (folosite în view-uri).</summary>
public static class Formatare
{
    private static readonly NumberFormatInfo FormatNumar = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalDigits = 0
    };

    /// <summary>1134 → "1.134".</summary>
    public static string Numar(int valoare) => valoare.ToString("N0", FormatNumar);

    /// <summary>0 → "Gratuit", 1134 → "1.134 lei".</summary>
    public static string Pret(int pret) => pret == 0 ? "Gratuit" : $"{Numar(pret)} lei";

    /// <summary>1 → "1 lună", 3 → "3 luni".</summary>
    public static string Perioada(int luni) => luni == 1 ? "1 lună" : $"{luni} luni";
}