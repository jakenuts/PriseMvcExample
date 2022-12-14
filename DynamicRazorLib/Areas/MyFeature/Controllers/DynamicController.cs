using Microsoft.AspNetCore.Mvc;

namespace DynamicRazorLib.Areas.MyFeature.Controllers
{
    [Area("MyFeature")]
    public class DynamicController : Controller
    {
        [Route("Dynamic")]
        public IActionResult Dynamic() => View("DynamicView");


        [Route("DynamicContent")]
        public IActionResult DynamicContent() => Content("This works!");
    }
}