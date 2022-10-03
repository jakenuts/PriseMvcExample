using Microsoft.AspNetCore.Mvc;

namespace DynamicRazorLib.Areas.MyFeature.Controllers
{
    [Route("Alt")]
    public class AltDynamicController : Controller
    {
        [Route("Dynamic")]
        public IActionResult Dynamic() => View("AltDynamicView");

        [Route("DynamicContentResult")]
        public IActionResult DynamicContent() => Content("This works!");
    }
}