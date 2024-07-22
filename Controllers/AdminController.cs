using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FoodDelivery.Models;
using FoodDelivery.CustomAttribute;
using MongoDB.Bson;

namespace FoodDelivery.Controllers
{
    [CustomAuthorizeAttribute(RoleEnum.Admin)]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var allRoles = Enum.GetValues(typeof(RoleEnum)).Cast<RoleEnum>().ToList();
            var userRoles = new Dictionary<ObjectId, RoleEnum>();

            foreach (var user in users)
            {
                userRoles[user.Id] = user.UserRole;
            }

            ViewBag.AllRoles = allRoles;
            ViewBag.Roles = userRoles;

            return View(users);
        }

        [HttpPost("ChangeUserRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(ObjectId userId, RoleEnum newRole)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.UserRole = newRole;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to change user role.");
            }

            return RedirectToAction("Users");
        }


        [HttpPost("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(ObjectId id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Unable to delete user");
            }

            return RedirectToAction("Users");
        }


        [HttpPost("ToggleLockout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(ObjectId id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }

            return RedirectToAction("Users");
        }
    }
}
