using FoodDelivery.Models;
using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.ViewModel
{
    public class OrderConfirmationViewModel
    {
        [Required]
        public Order? Order { get; set; }
        public List<CreditCard>? CreditCards { get; set; }
        public string? CreditCardId { get; set; }

        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }

        [Display(Name = "Card Holder Name")]
        public string? CardHolderName { get; set; }

        [Display(Name = "Expiry Month")]
        public string? ExpiryMonth { get; set; }

        [Display(Name = "Expiry Year")]
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
    }
}
