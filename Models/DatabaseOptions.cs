namespace TransAbon.Models;

/// <summary>Echivalentul setărilor din db.php. Enabled = DB_ENABLED.</summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool Enabled { get; set; }
    public string Host { get; set; } = "localhost";
    public string Name { get; set; } = "transabon";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConnectionString { get; set; } = "";
}
