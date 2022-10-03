using Microsoft.AspNetCore.Mvc;

namespace DynamicRazorLib.Areas.MyFeature.Controllers
{
    [Area("MyFeature")]
    public class DynamicController : Controller
    {
        [Route("Dynamic")]
        public IActionResult Dynamic()
        {
            return View("DynamicView");
        }

        [Route("DynamicContentResult")]
        public IActionResult DynamicContent()
        {
            return Content("This works!");
        }
    }
}
