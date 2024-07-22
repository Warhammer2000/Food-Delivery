using Microsoft.AspNetCore.Mvc;
using FoodDelivery.Models;
using FoodDelivery.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoodDelivery.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            ViewBag.CartItems = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("CartItems");
        }
    }
}
