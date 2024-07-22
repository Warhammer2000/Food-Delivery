using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FoodDelivery.Models;
using FoodDelivery.RabbitMQ;
using Microsoft.AspNetCore.Identity;
using FoodDelivery.Helpers;

namespace FoodDelivery.Controllers
{
    [Route("Orders")]
    public class OrdersController : Controller
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<Restaurant> _restaurants;
        private readonly IMongoCollection<MenuItem> _menuItems;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRabbitMqService _rabbitMqService;

        public OrdersController(IMongoDatabase database, UserManager<ApplicationUser> userManager, IRabbitMqService rabbitMqService)
        {
            _orders = database.GetCollection<Order>("Orders");
            _restaurants = database.GetCollection<Restaurant>("Restaurants");
            _menuItems = database.GetCollection<MenuItem>("MenuItems");
            _userManager = userManager;
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var restaurants = await _restaurants.Find(r => true).ToListAsync();
            return View(restaurants);
        }

        [HttpGet("Menu/{restaurantId}")]
        public async Task<IActionResult> Menu(string restaurantId)
        {
            var restaurant = await _restaurants.Find(r => r.Id == restaurantId).FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }
            var menuItems = await _menuItems.Find(m => m.RestaurantId == restaurantId).ToListAsync();
            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.RestaurantId = restaurant.Id;
            return View(menuItems);
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

            HttpContext.Session.SetObjectAsJson("CartItems", cartItems);

            return RedirectToAction("Menu", new { restaurantId });
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }
            return View(order);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }
            return View(order);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Order orderIn)
        {
            if (id != orderIn.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _orders.ReplaceOneAsync(o => o.Id == id, orderIn);
                _rabbitMqService.PublishMessage($"Order updated: {orderIn.Id}", "order_queue");
                return RedirectToAction(nameof(Index));
            }
            return View(orderIn);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }
            return View(order);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return RedirectToAction("NotFoundPage", "Error");
            }

            await _orders.DeleteOneAsync(o => o.Id == id);
            _rabbitMqService.PublishMessage($"Order deleted: {order.Id}", "order_queue");
            return RedirectToAction(nameof(Index));
        }
    }
}
