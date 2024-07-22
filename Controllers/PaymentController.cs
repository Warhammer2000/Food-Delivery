using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FoodDelivery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Stripe;
using FoodDelivery.Helpers;
using FoodDelivery.ViewModel;

namespace FoodDelivery.Controllers
{
    [Route("Payment")]
    public class PaymentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<CreditCard> _creditCards;
        private readonly StripeSettings _stripeSettings;

        public PaymentController(IMongoDatabase database, UserManager<ApplicationUser> userManager, IOptions<StripeSettings> stripeSettings)
        {
            _orders = database.GetCollection<Order>("Orders");
            _creditCards = database.GetCollection<CreditCard>("CreditCards");
            _userManager = userManager;
            _stripeSettings = stripeSettings.Value;

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        [HttpPost("ConfirmOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(OrderConfirmationViewModel viewModel, string stripeToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            if (string.IsNullOrEmpty(viewModel.Order.Id))
            {
                return RedirectToAction("NotFoundPage", "Error");
            }

            var existingOrder = await _orders.Find(o => o.Id == viewModel.Order.Id && o.UserId == user.Id.ToString()).FirstOrDefaultAsync();
            if (existingOrder == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }

            CreditCard selectedCard = null;

            if (string.IsNullOrEmpty(viewModel.CreditCardId))
            {
                selectedCard = new CreditCard
                {
                    UserId = user.Id.ToString(),
                    CardNumber = viewModel.CardNumber,
                    CardHolderName = viewModel.CardHolderName,
                    ExpiryMonth = viewModel.ExpiryMonth,
                    ExpiryYear = viewModel.ExpiryYear
                };
                await _creditCards.InsertOneAsync(selectedCard);
            }
            else
            {
                selectedCard = await _creditCards.Find(c => c.Id == viewModel.CreditCardId && c.UserId == user.Id.ToString()).FirstOrDefaultAsync();
                if (selectedCard == null)
                {
                    return RedirectToAction("NotFoundPage", "Error");
                }
            }
            try
            {
                // Если stripeToken не пустой, используем его для выполнения платежа
                if (!string.IsNullOrEmpty(stripeToken))
                {
                    var options = new ChargeCreateOptions
                    {
                        Amount = (long)(existingOrder.TotalPrice * 100), // amount in kopiykas
                        Currency = "rub",
                        Description = "Food Delivery Charge",
                        Source = stripeToken,
                    };

                    var service = new ChargeService();
                    Charge charge = await service.CreateAsync(options);

                    if (charge.Status != "succeeded")
                    {
                        throw new Exception("Payment failed");
                    }
                }

                // Если stripeToken пустой, значит используется сохраненная карта
                else
                {
                    var card = await _creditCards.Find(c => c.Id == viewModel.CreditCardId && c.UserId == user.Id.ToString()).FirstOrDefaultAsync();
                    if (card == null)
                    {
                        return RedirectToAction("NotFoundPage", "Error");
                    }

                    // Здесь вы можете добавить логику для обработки платежа с использованием сохраненной карты
                    // Для этого потребуется сохранение идентификатора карты (card.Id) и использование его в платежной системе
                }

                existingOrder.Status = "Confirmed";
                await _orders.ReplaceOneAsync(o => o.Id == existingOrder.Id, existingOrder);

                return RedirectToAction("OrderSuccess");
            }
            catch (Exception ex)
            {
                // Handle error
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }
        [HttpGet("OrderSuccess")]
        public IActionResult OrderSuccess()
        {
            return View();
        }

        private string CreateTokenForCard(CreditCard card)
        {
            var options = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Number = card.CardNumber,
                    ExpMonth = card.ExpiryMonth, 
                    ExpYear = card.ExpiryYear,   
                    Cvc = "123"
                }
            };

            var service = new TokenService();
            Token stripeToken = service.Create(options);
            return stripeToken.Id;
        }

    }
}
