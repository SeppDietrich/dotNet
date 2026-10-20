using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class RegisterViewModel
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

    [Required(ErrorMessage = "Introdu parola.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola trebuie să aibă minim 8 caractere.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirmă parola.")]
    [Compare(nameof(Password), ErrorMessage = "Parolele nu coincid.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmă parola")]
    public string ConfirmPassword { get; set; } = "";
}
