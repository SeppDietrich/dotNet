using TransAbon.Models;

namespace TransAbon.Services;

public interface ICatalogService
{
    /// <summary>
    /// Catalogul public din baza de date: doar categoriile active, cu tipurile active.
    /// Categoriile fără niciun tip activ nu apar (nu pot fi comandate).
    /// </summary>
    Task<CatalogPublicViewModel> GetPublicAsync();
}