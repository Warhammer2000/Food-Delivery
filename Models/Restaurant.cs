using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Models
{
    public class Restaurant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
            
        public string Address { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
        public double Rating { get; set; }
        public List<MenuItem> MenuItems { get; set; }

        [Required]
        public List<string> Categories { get; set; }
    }
}
