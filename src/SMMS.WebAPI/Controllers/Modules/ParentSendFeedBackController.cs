using Microsoft.AspNetCore.Mvc;

namespace SMMS.WebAPI.Controllers.Modules;
public class ParentSendFeedBackController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
