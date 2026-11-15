using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class AdminCatalogViewModel
{
    public List<Categorie> Categorii { get; set; } = new();
    public CategorieFormViewModel FormularCategorie { get; set; } = new();
    public TipAbonamentFormViewModel FormularTip { get; set; } = new();
}

public class CategorieFormViewModel
{
    [Required(ErrorMessage = "Numele categoriei este obligatoriu.")]
    [StringLength(200, ErrorMessage = "Numele poate avea cel mult 200 de caractere.")]
    [Display(Name = "Nume")]
    public string Nume { get; set; } = "";

    [StringLength(500, ErrorMessage = "Descrierea poate avea cel mult 500 de caractere.")]
    [Display(Name = "Descriere")]
    public string? Descriere { get; set; }
}

public class TipAbonamentFormViewModel
{
    [Required(ErrorMessage = "Alege categoria.")]
    [Display(Name = "Categorie")]
    public int? CategorieId { get; set; }

    [Required(ErrorMessage = "Durata este obligatorie.")]
    [Range(1, 120, ErrorMessage = "Durata trebuie să fie între 1 și 120 de luni.")]
    [Display(Name = "Durata (luni)")]
    public int? DurataLuni { get; set; }

    [Required(ErrorMessage = "Prețul este obligatoriu.")]
    [Range(0, 1_000_000, ErrorMessage = "Prețul trebuie să fie un număr întreg ≥ 0.")]
    [Display(Name = "Preț (lei)")]
    public int? Pret { get; set; }
}

public class PretFormViewModel
{
    [Required(ErrorMessage = "Prețul este obligatoriu.")]
    [Range(0, 1_000_000, ErrorMessage = "Prețul trebuie să fie un număr întreg ≥ 0.")]
    public int? Pret { get; set; }
}
