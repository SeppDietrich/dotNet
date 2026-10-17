using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication5.Models;

namespace WebApplication5.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Message"] = "Welcome to ASP.NET MVC!";
        return View();
    }
    

    public IActionResult Privacy()
    {
        ViewData["MessageSecond"] = "Welcome to ASP.NET MVC!.2";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}