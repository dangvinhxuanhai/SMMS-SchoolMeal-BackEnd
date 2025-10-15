using Microsoft.AspNetCore.Mvc;

namespace SMMS.WebAPI.Controllers.Modules.Parent;
public class ParentViewMealScheduleController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
