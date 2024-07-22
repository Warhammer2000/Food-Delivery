using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FoodDelivery.Models;
using FoodDelivery.RabbitMQ;
using FoodDelivery.Services;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using MongoDB.Bson;

namespace FoodDelivery.Controllers
{
    [Route("Restaurants")]
    public class RestaurantsController : Controller
    {
        private readonly IMongoCollection<Restaurant> _restaurants;
        private readonly IMongoCollection<MenuItem> _menuItems;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ImageService _imageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RestaurantsController> _logger;

        public RestaurantsController(IMongoDatabase database,
            IRabbitMqService rabbitMqService, ImageService imageService,
            IConfiguration configuration, ILogger<RestaurantsController> logger)
        {
            _restaurants = database.GetCollection<Restaurant>("Restaurants");
            _menuItems = database.GetCollection<MenuItem>("MenuItems");
            _rabbitMqService = rabbitMqService;
            _imageService = imageService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var restaurants = await _restaurants.Find(r => true).ToListAsync();
            return View(restaurants);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var restaurant = await _restaurants.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return NotFound();
            }

            var menuItems = await _menuItems.Find(m => m.RestaurantId == id).ToListAsync();
            restaurant.MenuItems = menuItems;

            return View(restaurant);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Restaurant restaurant)
        {
            if (string.IsNullOrEmpty(restaurant.Id))
            {
                restaurant.Id = ObjectId.GenerateNewId().ToString();
            }

            restaurant.Rating = 0;
            restaurant.MenuItems = new List<MenuItem>();
            restaurant.Categories = restaurant.Categories ?? new List<string>();
            await _restaurants.InsertOneAsync(restaurant);
            _rabbitMqService.PublishMessage($"Restaurant created: {restaurant.Name}", "restaurant_queue");
            return RedirectToAction(nameof(Index));

        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var restaurant = await _restaurants.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Restaurant restaurantIn)
        {
            if (id != restaurantIn.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _restaurants.ReplaceOneAsync(r => r.Id == id, restaurantIn);
                _rabbitMqService.PublishMessage($"Restaurant updated: {restaurantIn.Name}", "restaurant_queue");
                return RedirectToAction(nameof(Index));
            }
            return View(restaurantIn);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var restaurant = await _restaurants.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var restaurant = await _restaurants.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return NotFound();
            }

            await _restaurants.DeleteOneAsync(r => r.Id == id);
            _rabbitMqService.PublishMessage($"Restaurant deleted: {restaurant.Name}", "restaurant_queue");
            return RedirectToAction(nameof(Index));
        }


        [HttpGet("AddMenuItem/{restaurantId}")]
        public IActionResult AddMenuItem(string restaurantId)
        {
            var menuItem = new MenuItem { RestaurantId = restaurantId };
            return View(menuItem);
        }

        [HttpPost("AddMenuItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMenuItem(MenuItem menuItem, IFormFile imageFile, string imageUrl)
        {
            if (string.IsNullOrEmpty(menuItem.Id))
            {
                menuItem.Id = ObjectId.GenerateNewId().ToString();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                menuItem.ImageUrl = await _imageService.UploadImageAsync(filePath);
            }
            else if (!string.IsNullOrEmpty(imageUrl))
            {
                menuItem.ImageUrl = await _imageService.UploadImageFromUrlAsync(imageUrl);
            }

            await _menuItems.InsertOneAsync(menuItem);
            _rabbitMqService.PublishMessage($"Menu item added: {menuItem.Name}", "menuitem_queue");
            return RedirectToAction(nameof(Details), new { id = menuItem.RestaurantId });
        }


        [HttpPost("Rate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(string id, double rating)
        {
            var restaurant = await _restaurants.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return NotFound();
            }
            restaurant.Rating = (restaurant.Rating + rating) / 2; // Простое усреднение рейтинга
            await _restaurants.ReplaceOneAsync(r => r.Id == id, restaurant);
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpGet("SearchImage")]
        public async Task<IActionResult> SearchImage(MenuItem menuItem)
        {
            if (string.IsNullOrEmpty(menuItem.ImageSearchQuery))
            {
                ModelState.AddModelError(string.Empty, "Query cannot be empty.");
                return View("AddMenuItem", menuItem); // Возвращаем модель с текущими значениями
            }

            try
            {
                var client = new HttpClient();
                var accessKey = _configuration["Unsplash:AccessKey"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", accessKey);

                var response = await client.GetAsync($"https://api.unsplash.com/search/photos?query={menuItem.ImageSearchQuery}");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Unsplash API request failed: {response.StatusCode}, Response: {responseBody}");
                    ModelState.AddModelError(string.Empty, "Failed to retrieve images. Please try again later.");
                    return View("AddMenuItem", menuItem); // Возвращаем модель с текущими значениями
                }

                var jsonResponse = JObject.Parse(responseBody);
                var images = jsonResponse["results"].Select(r => r["urls"]["small"].ToString()).ToList();
                ViewBag.Images = images;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Request Exception occurred while searching images.");
                ModelState.AddModelError(string.Empty, "Failed to retrieve images. Please check your network connection and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while searching images.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            }

            return View("AddMenuItem", menuItem);
        }
    }
}
