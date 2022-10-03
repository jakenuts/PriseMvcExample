using Microsoft.AspNetCore.Mvc;

namespace StaticRazorLib.Controllers
{
    [Area("MyFeature")]
    public class StaticController : Controller
    {
        [Route("Static")]
        public IActionResult Static() => View("StaticView");
    }
}