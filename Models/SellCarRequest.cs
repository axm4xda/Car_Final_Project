namespace Car_Project.Models
{
    public class SellCarRequest : BaseEntity
    {
        public string OwnerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CarTitle { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public decimal AskingPrice { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsReviewed { get; set; }
    }
}
