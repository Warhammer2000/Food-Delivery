using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FoodDelivery.Models;
using Microsoft.AspNetCore.Identity;
using FoodDelivery.Helpers;
using Microsoft.Extensions.Options;
using Stripe;


namespace FoodDelivery.Controllers
{
    [Route("CreditCard")]
    public class CreditCardController : Controller
    {
        private readonly IMongoCollection<CreditCard> _creditCards;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StripeSettings _stripeSettings;
        public CreditCardController(IMongoDatabase database,
            UserManager<ApplicationUser> userManager, IOptions<StripeSettings> stripeSettings)
        {
            _creditCards = database.GetCollection<CreditCard>("CreditCards");
            _userManager = userManager;
            _stripeSettings = stripeSettings.Value;

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            var creditCards = await _creditCards.Find(c => c.UserId == user.Id.ToString()).ToListAsync();
            return View(creditCards);
        }

        [HttpGet("Add")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CreditCard creditCard)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            if (ModelState.IsValid)
            {
                creditCard.UserId = user.Id.ToString();
                await _creditCards.InsertOneAsync(creditCard);
                return RedirectToAction("Index");
            }

            return View(creditCard);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            var creditCard = await _creditCards.Find(c => c.Id == id && c.UserId == user.Id.ToString()).FirstOrDefaultAsync();
            if (creditCard == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }

            return View(creditCard);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            var creditCard = await _creditCards.Find(c => c.Id == id && c.UserId == user.Id.ToString()).FirstOrDefaultAsync();
            if (creditCard == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }

            await _creditCards.DeleteOneAsync(c => c.Id == id);
            return RedirectToAction("Index");
        }
    }
}
