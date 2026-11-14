using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class AdminCatalogViewModel
{
    public List<CategorieCatalogItem> Categorii { get; set; } = [];
    public CategorieFormViewModel FormCategorie { get; set; } = new();
    public TipFormViewModel FormTip { get; set; } = new();
}

public class CategorieCatalogItem
{
    public int Id { get; set; }
    public string Nume { get; set; } = "";
    public string Descriere { get; set; } = "";
    public bool Activ { get; set; }
    public List<TipCatalogItem> Tipuri { get; set; } = [];
}

public class TipCatalogItem
{
    public int Id { get; set; }
    public int DurataLuni { get; set; }
    public int Pret { get; set; }
    public bool Activ { get; set; }
}

/// <summary>Formularul „Adaugă categorie”.</summary>
public class CategorieFormViewModel
{
    [Required(ErrorMessage = "Introdu numele categoriei.")]
    [StringLength(200, ErrorMessage = "Numele categoriei poate avea cel mult 200 de caractere.")]
    [Display(Name = "Nume")]
    public string Nume { get; set; } = "";

    [StringLength(500, ErrorMessage = "Descrierea poate avea cel mult 500 de caractere.")]
    [Display(Name = "Descriere")]
    public string? Descriere { get; set; }
}

/// <summary>Modificarea numelui sau a descrierii unei categorii existente.</summary>
public class CategorieEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Introdu numele categoriei.")]
    [StringLength(200, ErrorMessage = "Numele categoriei poate avea cel mult 200 de caractere.")]
    public string Nume { get; set; } = "";

    [StringLength(500, ErrorMessage = "Descrierea poate avea cel mult 500 de caractere.")]
    public string? Descriere { get; set; }
}

/// <summary>Formularul „Adaugă tip de abonament”.</summary>
public class TipFormViewModel
{
    [Required(ErrorMessage = "Alege categoria.")]
    [Display(Name = "Categorie")]
    public int? CategorieId { get; set; }

    [Required(ErrorMessage = "Introdu durata în luni.")]
    [Range(1, 60, ErrorMessage = "Durata trebuie să fie un număr întreg între 1 și 60 de luni.")]
    [Display(Name = "Durată (luni)")]
    public int? DurataLuni { get; set; }

    [Required(ErrorMessage = "Introdu prețul în lei (0 pentru abonament gratuit).")]
    [Range(0, 1_000_000, ErrorMessage = "Prețul trebuie să fie un număr întreg între 0 și 1.000.000 lei.")]
    [Display(Name = "Preț (lei)")]
    public int? Pret { get; set; }
}

/// <summary>Modificarea prețului unui tip existent.</summary>
public class TipPretEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Introdu prețul în lei (0 pentru abonament gratuit).")]
    [Range(0, 1_000_000, ErrorMessage = "Prețul trebuie să fie un număr întreg între 0 și 1.000.000 lei.")]
    public int? Pret { get; set; }
}