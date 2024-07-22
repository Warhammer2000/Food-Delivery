using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/NotFound")]
        public IActionResult NotFoundPage()
        {
            return View();
        }

        [Route("Error/Unauthorized")]
        public IActionResult UnauthorizedPage()
        {
            return View();
        }
    }
}
