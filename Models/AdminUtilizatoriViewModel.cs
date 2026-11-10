using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class AdminUtilizatoriViewModel
{
    public string? Cautare { get; set; }

    public IReadOnlyList<UtilizatorItemViewModel> Utilizatori { get; set; } = [];

    public CreareControlorViewModel FormularControlor { get; set; } = new();
}

public class UtilizatorItemViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string NumeComplet { get; set; } = "";
    public string Rol { get; set; } = "";
    public string? CodLegitimatie { get; set; }
    public bool Activ { get; set; }
    public DateTime DataInregistrarii { get; set; }
}

public class CreareControlorViewModel
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

    [Required(ErrorMessage = "Introdu codul de legitimație.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Codul de legitimație trebuie să aibă între 2 și 50 de caractere.")]
    [Display(Name = "Cod de legitimație")]
    public string CodLegitimatie { get; set; } = "";

    [Required(ErrorMessage = "Introdu parola inițială.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola trebuie să aibă minim 8 caractere.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă inițială")]
    public string Password { get; set; } = "";
}
