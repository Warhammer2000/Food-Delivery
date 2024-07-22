using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FoodDelivery.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string? RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }

        public DateTime? OrderDate { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
        public string? Status { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal? TotalPrice { get; set; }
    }

}
