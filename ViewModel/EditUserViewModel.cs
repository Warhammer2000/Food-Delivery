namespace FoodDelivery.ViewModel
{
    public class EditUserViewModel
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public IEnumerable<string>? Roles { get; set; }
    }
}
