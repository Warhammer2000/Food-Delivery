using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FoodDelivery.Models
{
    public class MenuItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public string RestaurantId { get; set; }

        [BsonIgnore]
        public Restaurant Restaurant { get; set; }

        [BsonIgnore]
        public string ImageSearchQuery { get; set; }
    }

}
