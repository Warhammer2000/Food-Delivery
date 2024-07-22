using FoodDelivery.Models;

namespace FoodDelivery.ViewModel
{
    public class CheckoutViewModel
    {
        public ApplicationUser? User { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
