namespace TransAbon.Models;

/// <summary>Informațiile afișate în footer (secțiunea „AppInfo” din appsettings.json).</summary>
public class AppInfoOptions
{
    public const string SectionName = "AppInfo";

    public string Autor { get; set; } = "";
    public string Versiune { get; set; } = "";
    public string Grupa { get; set; } = "";
}
