using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}