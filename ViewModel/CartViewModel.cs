using FoodDelivery.Models;

namespace FoodDelivery.ViewModel
{
    public class CartViewModel
    {
        public List<OrderItem> OrderItems { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
