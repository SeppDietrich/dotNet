using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

/// <summary>
/// Starea afișată în interfață, dedusă din stare și din data curentă:
/// Anulat (dacă Stare == Anulat),
/// Programat (dacă azi < DataInceput),
/// Expirat (dacă azi > DataExpirare sau Stare == Expirat),
/// Activ (dacă DataInceput <= azi <= DataExpirare și Stare == Activ).
/// </summary>
public enum StareAbonamentAfisata
{
    Activ,
    Programat,
    Expirat,
    Anulat
}

public class ClientDashboardViewModel
{
    public List<AbonamentClientItemViewModel> Abonamente { get; set; } = [];
    public List<OptiuneTipAbonamentViewModel> TipuriDisponibile { get; set; } = [];
    public List<CategorieOptiuneViewModel> CategoriiDisponibile { get; set; } = [];
    public PlasareComandaViewModel FormularComanda { get; set; } = new();
}

public class CategorieOptiuneViewModel
{
    public int Id { get; set; }
    public string Nume { get; set; } = "";
    public string Descriere { get; set; } = "";
}

public class OptiuneTipAbonamentViewModel
{
    public int Id { get; set; }
    public int CategorieId { get; set; }
    public string CategorieNume { get; set; } = "";
    public int DurataLuni { get; set; }
    public string DurataText => DurataLuni == 1 ? "1 lună" : $"{DurataLuni} luni";
    public int Pret { get; set; }
    public bool EsteGratuit => Pret == 0;
    public string EtichetaSelect => EsteGratuit
        ? $"{CategorieNume} – {DurataText} (Gratuit)"
        : $"{CategorieNume} – {DurataText} ({Pret} MDL)";
}

public class AbonamentClientItemViewModel
{
    public int Id { get; set; }
    public string Cod { get; set; } = "";
    public string Categorie { get; set; } = "";
    public int DurataLuni { get; set; }
    public string DurataText => DurataLuni == 1 ? "1 lună" : $"{DurataLuni} luni";
    public int Pret { get; set; }
    public bool EsteGratuit => Pret == 0;
    public DateOnly DataInceput { get; set; }
    public DateOnly DataExpirare { get; set; }
    public StareAbonament StareMemorata { get; set; }
    public StareAbonamentAfisata StareCalculata { get; set; }
    public bool PoateFiAnulat { get; set; }

    public string BadgeClass => StareCalculata switch
    {
        StareAbonamentAfisata.Activ => "badge-activ",
        StareAbonamentAfisata.Programat => "badge-programat",
        StareAbonamentAfisata.Expirat => "badge-expirat",
        StareAbonamentAfisata.Anulat => "badge-anulat",
        _ => "badge-inexistent"
    };

    public string StareText => StareCalculata switch
    {
        StareAbonamentAfisata.Activ => "activ",
        StareAbonamentAfisata.Programat => "programat",
        StareAbonamentAfisata.Expirat => "expirat",
        StareAbonamentAfisata.Anulat => "anulat",
        _ => "necunoscut"
    };
}

public class PlasareComandaViewModel
{
    [Display(Name = "Tip abonament")]
    public int TipAbonamentId { get; set; }

    [Display(Name = "Categorie")]
    public int CategorieId { get; set; }

    [Display(Name = "Durată (luni)")]
    public int DurataLuni { get; set; }

    [Required(ErrorMessage = "Selectează data de început.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de început")]
    public DateOnly DataInceput { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
