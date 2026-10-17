using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class FeedbackViewModel
{
    [Required(ErrorMessage = "Introdu numele (minim 2 caractere).")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Introdu numele (minim 2 caractere).")]
    [Display(Name = "Nume")]
    public string Nume { get; set; } = "";

    [Required(ErrorMessage = "Introdu o adresă de e-mail validă.")]
    [EmailAddress(ErrorMessage = "Introdu o adresă de e-mail validă.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Alege o evaluare de la 1 la 5.")]
    [Range(1, 5, ErrorMessage = "Alege o evaluare de la 1 la 5.")]
    [Display(Name = "Evaluare")]
    public int? Evaluare { get; set; }

    [Required(ErrorMessage = "Mesajul trebuie să aibă minim 10 caractere.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mesajul trebuie să aibă minim 10 caractere.")]
    [Display(Name = "Mesaj")]
    public string Mesaj { get; set; } = "";
}
