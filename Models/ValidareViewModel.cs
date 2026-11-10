using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class ValidareViewModel
{
    [Required(ErrorMessage = "Introdu codul abonamentului.")]
    [StringLength(64, ErrorMessage = "Codul introdus este prea lung (maxim 64 caractere).")]
    [Display(Name = "Codul abonamentului")]
    public string Cod { get; set; } = "";

    public ValidareRezultatItem? RezultatCurent { get; set; }

    public IReadOnlyList<ValidareIstoricItem> ValidariAzi { get; set; } = [];
}

public class ValidareRezultatItem
{
    public string CodIntrodus { get; set; } = "";
    public RezultatControl Rezultat { get; set; }
    public bool EsteProgramat { get; set; }
    public bool EsteValid => Rezultat == RezultatControl.Valid && !EsteProgramat;
    public string TitluAfisat { get; set; } = "";
    public string Detalii { get; set; } = "";
}

public class ValidareIstoricItem
{
    public DateTime DataOra { get; set; }
    public string CodIntrodus { get; set; } = "";
    public RezultatControl Rezultat { get; set; }
    public bool EsteProgramat { get; set; }
    public string EtichetaRezultat { get; set; } = "";
    public string ClasaBadge { get; set; } = "";
}
