namespace Car_Project.Models
{
    public enum FuelType
    {
        Petrol,
        Diesel,
        Electric,
        Hybrid
    }

    public enum TransmissionType
    {
        Manual,
        Automatic
    }

    public enum CarCondition
    {
        New,
        Used
    }

    public class Car : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public FuelType FuelType { get; set; }
        public TransmissionType Transmission { get; set; }
        public CarCondition Condition { get; set; }
        public string? BodyStyle { get; set; }
        public string? DriveType { get; set; }
        public string? Color { get; set; }
        public string? InteriorColor { get; set; }
        public int? Cylinders { get; set; }
        public int? DoorCount { get; set; }
        public string? Description { get; set; }

        /// <summary>Birba?a thumbnail URL — CarImage c?dv?lind?n müst?qil fallback ??kil</summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>"Special", "Great Price" kimi badge m?tni</summary>
        public string? Badge { get; set; }

        /// <summary>"bg-primary-2", "bg-green" kimi CSS sinfi</summary>
        public string? BadgeColor { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        public ICollection<CarImage> Images { get; set; } = new List<CarImage>();
        public ICollection<CarFeatureMapping> Features { get; set; } = new List<CarFeatureMapping>();
    }
}
