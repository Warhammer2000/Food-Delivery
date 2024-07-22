using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FoodDelivery.Models;
using FoodDelivery.RabbitMQ;

namespace FoodDelivery.Controllers
{
    [Route("Deliveries")]
    public class DeliveriesController : Controller
    {
        private readonly IMongoCollection<Delivery> _deliveries;
        private readonly IRabbitMqService _rabbitMqService;

        public DeliveriesController(IMongoDatabase database, IRabbitMqService rabbitMqService)
        {
            _deliveries = database.GetCollection<Delivery>("Deliveries");
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var deliveries = await _deliveries.Find(d => true).ToListAsync();
            return View(deliveries);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var delivery = await _deliveries.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (delivery == null)
            {
                return NotFound();
            }
            return View(delivery);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                await _deliveries.InsertOneAsync(delivery);
                _rabbitMqService.PublishMessage($"Delivery created: {delivery.Id}", "delivery_queue");
                return RedirectToAction(nameof(Index));
            }
            return View(delivery);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var delivery = await _deliveries.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (delivery == null)
            {
                return NotFound();
            }
            return View(delivery);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Delivery deliveryIn)
        {
            if (id != deliveryIn.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _deliveries.ReplaceOneAsync(d => d.Id == id, deliveryIn);
                _rabbitMqService.PublishMessage($"Delivery updated: {deliveryIn.Id}", "delivery_queue");
                return RedirectToAction(nameof(Index));
            }
            return View(deliveryIn);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var delivery = await _deliveries.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (delivery == null)
            {
                return NotFound();
            }
            return View(delivery);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var delivery = await _deliveries.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (delivery == null)
            {
                return NotFound();
            }

            await _deliveries.DeleteOneAsync(d => d.Id == id);
            _rabbitMqService.PublishMessage($"Delivery deleted: {delivery.Id}", "delivery_queue");
            return RedirectToAction(nameof(Index));
        }
    }
}
