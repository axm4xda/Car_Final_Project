using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    [Route("404Error")]
    public class ErrorController : Controller
    {
        [Route("Index")]
        [Route("")]
        public IActionResult Index()
        {
            return View("~/Views/404Error/Index.cshtml");
        }
    }
}
