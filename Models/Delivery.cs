using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FoodDelivery.Models
{
    public class Delivery
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string OrderId { get; set; }
        public Order Order { get; set; }

        public string CourierId { get; set; }
        public ApplicationUser Courier { get; set; }

        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; }
    }

}
