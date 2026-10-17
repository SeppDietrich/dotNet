using System.ComponentModel.DataAnnotations;

namespace TransAbon.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Completează e-mailul.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Completează parola.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă")]
    public string Password { get; set; } = "";
}
