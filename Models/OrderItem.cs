using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FoodDelivery.Models
{
    public class OrderItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}
