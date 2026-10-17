namespace TransAbon.Models;

/// <summary>Echivalentul constantelor din config.php (AUTOR, VERSIUNE_APLICATIE, GRUPA).</summary>
public class AppInfoOptions
{
    public const string SectionName = "AppInfo";

    public string Autor { get; set; } = "";
    public string Versiune { get; set; } = "";
    public string Grupa { get; set; } = "";
}
