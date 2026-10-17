using Microsoft.AspNetCore.Mvc;

namespace TransAbon.Controllers;

public class AbonamenteController : Controller
{
    [HttpGet("abonamente")]
    public IActionResult Index() => View();
}
