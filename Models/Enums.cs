namespace TransAbon.Models;

/// <summary>Starea unei comenzi. Comanda confirmată emite imediat un abonament.</summary>
public enum StareComanda
{
    Confirmata,
    Anulata
}

/// <summary>
/// Starea memorată a unui abonament (caiet de sarcini, tabelul 3.3).
/// Atenție: „programat” (început în viitor) nu este stare memorată, se deduce din DataInceput.
/// </summary>
public enum StareAbonament
{
    Activ,
    Expirat,
    Anulat
}

/// <summary>Rezultatul unui control efectuat de un controlor (caiet de sarcini, 5.3).</summary>
public enum RezultatControl
{
    Valid,
    Expirat,
    Anulat,
    CodInexistent
}
