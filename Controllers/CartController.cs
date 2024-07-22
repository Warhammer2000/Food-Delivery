using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FoodDelivery.Models;
using FoodDelivery.Helpers;
using Microsoft.AspNetCore.Identity;
using FoodDelivery.ViewModel;

namespace FoodDelivery.Controllers
{
    [Route("Cart")]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<MenuItem> _menuItems;
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<Restaurant> _restaurants;
        private readonly IMongoCollection<CreditCard> _creditCards;

        public CartController(UserManager<ApplicationUser> userManager, IMongoDatabase database)
        {
            _userManager = userManager;
            _menuItems = database.GetCollection<MenuItem>("MenuItems");
            _creditCards = database.GetCollection<CreditCard>("CreditCards");
            _restaurants = database.GetCollection<Restaurant>("Restaurants");
            _orders = database.GetCollection<Order>("Orders");
        }

        [HttpPost("AddToCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string menuItemId, string restaurantId)
        {
            var menuItem = await _menuItems.Find(m => m.Id == menuItemId).FirstOrDefaultAsync();
            if (menuItem == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            var cartItems = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("CartItems") ?? new List<OrderItem>();

            var orderItem = cartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);
            if (orderItem == null)
            {
                orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    MenuItem = menuItem,
                    Quantity = 1,
                    Price = menuItem.Price
                };
                cartItems.Add(orderItem);
            }
            else
            {
                orderItem.Quantity++;
            }

            HttpContext.Session.SetObjectFromJson("CartItems", cartItems);

            return RedirectToAction("Menu", "Orders", new { restaurantId });
        }

        [HttpPost("RemoveFromCart")]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(string menuItemId)
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("CartItems") ?? new List<OrderItem>();
            var itemToRemove = cartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);

            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                HttpContext.Session.SetObjectFromJson("CartItems", cartItems);
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("CartItems") ?? new List<OrderItem>();
            var cartViewModel = new CartViewModel
            {
                OrderItems = cartItems,
                DeliveryAddress = string.Empty
            };
            return View(cartViewModel);
        }

        [HttpPost("Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CartViewModel cartViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            var cartItems = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("CartItems") ?? new List<OrderItem>();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                UserId = user.Id.ToString(),
                User = user,
                RestaurantId = cartItems.First().MenuItem.RestaurantId,
                Restaurant = await _restaurants.Find(r => r.Id == cartItems.First().MenuItem.RestaurantId).FirstOrDefaultAsync(),
                OrderDate = DateTime.UtcNow,
                OrderItems = cartItems,
                Status = "Pending",
                DeliveryAddress = cartViewModel.DeliveryAddress,
                TotalPrice = cartItems.Sum(i => i.Price * i.Quantity)
            };

            await _orders.InsertOneAsync(order);
            HttpContext.Session.Remove("CartItems");

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        [HttpGet("OrderConfirmation/{orderId}")]
        public async Task<IActionResult> OrderConfirmation(string orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("UnauthorizedPage", "Error");
            }

            var order = await _orders.Find(o => o.Id == orderId && o.UserId == user.Id.ToString()).FirstOrDefaultAsync();
            if (order == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }

            var creditCards = await _creditCards.Find(c => c.UserId == user.Id.ToString()).ToListAsync();

            var viewModel = new OrderConfirmationViewModel
            {
                Order = order,
                CreditCards = creditCards
            };

            return View(viewModel);
        }

        [HttpPost("ConfirmOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(OrderConfirmationViewModel viewModel)
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

            if (string.IsNullOrEmpty(viewModel.CreditCardId))
            {
                var newCard = new CreditCard
                {
                    UserId = user.Id.ToString(),
                    CardNumber = viewModel.CardNumber,
                    CardHolderName = viewModel.CardHolderName,
                    ExpiryMonth = viewModel.ExpiryMonth,
                    ExpiryYear = viewModel.ExpiryYear
                };
                await _creditCards.InsertOneAsync(newCard);
            }
            else
            {
                var card = await _creditCards.Find(c => c.Id == viewModel.CreditCardId && c.UserId == user.Id.ToString()).FirstOrDefaultAsync();
                if (card == null)
                {
                    return RedirectToAction("NotFoundPage", "Error");
                }
            }

            existingOrder.Status = "Confirmed";
            await _orders.ReplaceOneAsync(o => o.Id == existingOrder.Id, existingOrder);

            return RedirectToAction("OrderSuccess");
        }

        [HttpGet("OrderSuccess")]
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
