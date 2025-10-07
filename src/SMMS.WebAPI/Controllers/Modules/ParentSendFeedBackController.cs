using Microsoft.AspNetCore.Mvc;

namespace SMMS.WebAPI.Controllers.Modules;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
