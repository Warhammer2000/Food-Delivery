using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FoodDelivery.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.CustomAttribute
{
    public class CustomAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly RoleEnum _role;

        public CustomAuthorizeAttribute(RoleEnum role)
        {
            _role = role;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userManager = (UserManager<ApplicationUser>)context.HttpContext.RequestServices.GetService(typeof(UserManager<ApplicationUser>));
            var user = await userManager.GetUserAsync(context.HttpContext.User);

            if (user == null || user.UserRole != _role)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
