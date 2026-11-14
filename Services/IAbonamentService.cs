using TransAbon.Models;

namespace TransAbon.Services;

/// <summary>
/// Logica de business pentru abonamente: generare cod criptografic unic,
/// calculul datei de expirare, plasare comenzi și anulare.
/// </summary>
public interface IAbonamentService
{
    Task<string> GenereazaCodUnicAsync();
    DateOnly CalculeazaDataExpirare(DateOnly dataInceput, int durataLuni);
    Task<List<AbonamentClientItemViewModel>> ObtineAbonamenteClientAsync(string clientId);
    Task<List<OptiuneTipAbonamentViewModel>> ObtineTipuriActiveAsync();
    Task<List<CategorieOptiuneViewModel>> ObtineCategoriiActiveAsync();
    Task<(bool Succes, string? MesajEroare, Abonament? Abonament)> CreeazaAbonamentAsync(
        string clientId,
        int tipAbonamentId,
        int categorieId,
        int durataLuni,
        DateOnly dataInceput);
    Task<(bool Succes, string? MesajEroare, string? CodAbonament)> AnuleazaAbonamentAsync(
        string clientId,
        int abonamentId);
}
