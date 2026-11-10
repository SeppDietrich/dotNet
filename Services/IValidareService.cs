using TransAbon.Models;

namespace TransAbon.Services;

public interface IValidareService
{
    /// <summary>
    /// Caută abonamentul după cod (case-insensitive, fără spații la capete),
    /// calculează rezultatul, salvează înregistrarea Control și returnează rezultatul pentru afișare.
    /// </summary>
    Task<ValidareRezultatItem> VerificaCodAsync(string controlorId, string codIntrodus);

    /// <summary>
    /// Returnează validările efectuate de controlorul dat în ziua curentă (UTC), cele mai noi primele.
    /// </summary>
    Task<IReadOnlyList<ValidareIstoricItem>> ObtineValidariAziAsync(string controlorId);
}
