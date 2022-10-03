using Microsoft.AspNetCore.Mvc;

namespace DynamicRazorLib.Areas.MyFeature.Controllers
{
    [Route("Alt")]
    public class AltDynamicController : Controller
    {
        [Route("Dynamic")]
        public IActionResult Dynamic()
        {
            return View("AltDynamicView");
        }

        [Route("DynamicContentResult")]
        public IActionResult DynamicContent()
        {
            return Content("This works!");
        }
    }
}
