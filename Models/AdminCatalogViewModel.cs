using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class AdminCatalogViewModel
{
    public string? Cautare { get; set; }
    public List<Categorie> Categorii { get; set; } = new();
    public List<Categorie> ToateCategoriile { get; set; } = new();
    public CategorieFormViewModel FormularCategorie { get; set; } = new();
    public TipAbonamentFormViewModel FormularTip { get; set; } = new();
}

public class CategorieFormViewModel
{
    [Required(ErrorMessage = "Numele categoriei este obligatoriu.")]
    [StringLength(200, ErrorMessage = "Numele poate avea cel mult 200 de caractere.")]
    [Display(Name = "Nume *")]
    public string Nume { get; set; } = "";

    [StringLength(500, ErrorMessage = "Descrierea poate avea cel mult 500 de caractere.")]
    [Display(Name = "Descriere")]
    public string? Descriere { get; set; }
}

public class CategorieEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele categoriei este obligatoriu.")]
    [StringLength(200, ErrorMessage = "Numele poate avea cel mult 200 de caractere.")]
    [Display(Name = "Nume *")]
    public string Nume { get; set; } = "";

    [StringLength(500, ErrorMessage = "Descrierea poate avea cel mult 500 de caractere.")]
    [Display(Name = "Descriere")]
    public string? Descriere { get; set; }

    [Display(Name = "Activă")]
    public bool Activ { get; set; } = true;
}

public class CategorieDetaliiViewModel
{
    public required Categorie Categorie { get; set; }
    public int NumarComenzi { get; set; }
    public int NumarAbonamente { get; set; }
}

public class CategorieStergeViewModel
{
    public required Categorie Categorie { get; set; }
    public int NumarComenzi { get; set; }
    public int NumarAbonamente { get; set; }
    public bool PoateFiStearsa => NumarComenzi == 0 && NumarAbonamente == 0;
}

public class TipAbonamentFormViewModel
{
    [Required(ErrorMessage = "Alege categoria.")]
    [Display(Name = "Categorie *")]
    public int? CategorieId { get; set; }

    [Required(ErrorMessage = "Durata este obligatorie.")]
    [Range(1, 120, ErrorMessage = "Durata trebuie să fie între 1 și 120 de luni.")]
    [Display(Name = "Durata (luni) *")]
    public int? DurataLuni { get; set; }

    [Required(ErrorMessage = "Prețul este obligatoriu.")]
    [Range(0, 1_000_000, ErrorMessage = "Prețul trebuie să fie un număr întreg ≥ 0.")]
    [Display(Name = "Preț (lei) *")]
    public int? Pret { get; set; }
}

public class TipAbonamentEditViewModel
{
    public int Id { get; set; }
    public int CategorieId { get; set; }
    public string CategorieNume { get; set; } = "";

    [Required(ErrorMessage = "Durata este obligatorie.")]
    [Range(1, 120, ErrorMessage = "Durata trebuie să fie între 1 și 120 de luni.")]
    [Display(Name = "Durata (luni) *")]
    public int? DurataLuni { get; set; }

    [Required(ErrorMessage = "Prețul este obligatoriu.")]
    [Range(0, 1_000_000, ErrorMessage = "Prețul trebuie să fie un număr întreg ≥ 0.")]
    [Display(Name = "Preț (lei) *")]
    public int? Pret { get; set; }

    [Display(Name = "Activ")]
    public bool Activ { get; set; } = true;
}

public class TipAbonamentDetaliiViewModel
{
    public required TipAbonament Tip { get; set; }
    public int NumarComenzi { get; set; }
    public int NumarAbonamente { get; set; }
}

public class TipAbonamentStergeViewModel
{
    public required TipAbonament Tip { get; set; }
    public int NumarComenzi { get; set; }
    public int NumarAbonamente { get; set; }
    public bool PoateFiSters => NumarComenzi == 0 && NumarAbonamente == 0;
}

public class PretFormViewModel
{
    [Required(ErrorMessage = "Prețul este obligatoriu.")]
    [Range(0, 1_000_000, ErrorMessage = "Prețul trebuie să fie un număr întreg ≥ 0.")]
    public int? Pret { get; set; }
}

public class CatalogNotFoundViewModel
{
    public int Id { get; set; }
    public string TipEntitate { get; set; } = "Categorie";
}
