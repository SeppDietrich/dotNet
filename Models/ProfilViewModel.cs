using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class ProfilPaginaViewModel
{
    public ProfilDateViewModel Date { get; set; } = new();
    public SchimbaParolaViewModel Parola { get; set; } = new();
}

public class ProfilDateViewModel
{
    [Required(ErrorMessage = "Introdu numele (minim 2 caractere).")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Introdu numele (minim 2 caractere).")]
    [Display(Name = "Nume")]
    public string Nume { get; set; } = "";

    [Required(ErrorMessage = "Introdu prenumele (minim 2 caractere).")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Introdu prenumele (minim 2 caractere).")]
    [Display(Name = "Prenume")]
    public string Prenume { get; set; } = "";

    [Required(ErrorMessage = "Introdu o adresă de e-mail validă.")]
    [EmailAddress(ErrorMessage = "Introdu o adresă de e-mail validă.")]
    [StringLength(200, ErrorMessage = "Adresa de e-mail este prea lungă.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";
}

public class SchimbaParolaViewModel
{
    [Required(ErrorMessage = "Introdu parola curentă.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola curentă")]
    public string ParolaCurenta { get; set; } = "";

    [Required(ErrorMessage = "Introdu parola nouă.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola nouă trebuie să aibă minim 8 caractere.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola nouă (minim 8 caractere, literă mare, literă mică, cifră)")]
    public string ParolaNoua { get; set; } = "";

    [Required(ErrorMessage = "Confirmă parola nouă.")]
    [Compare(nameof(ParolaNoua), ErrorMessage = "Parolele noi nu coincid.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmă parola nouă")]
    public string ConfirmaParola { get; set; } = "";
}