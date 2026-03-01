using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.About;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class AboutController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IBrandService _brandService;

        public AboutController(IReviewService reviewService, IBrandService brandService)
        {
            _reviewService = reviewService;
            _brandService = brandService;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = (await _reviewService.GetTopRatedAsync(8))
                .Select(r => new AboutReviewViewModel
                {
                    AuthorName = r.AuthorName,
                    AuthorTitle = r.AuthorTitle,
                    AvatarUrl = r.AvatarUrl,
                    Content = r.Content,
                    Rating = r.Rating
                }).ToList();

            var brands = (await _brandService.GetAllAsync())
                .Select(b => new AboutBrandViewModel
                {
                    Name = b.Name,
                    LogoUrl = b.LogoUrl
                }).ToList();

            var vm = new AboutIndexViewModel
            {
                Reviews = reviews,
                Brands = brands,
                Stats = new List<StatCounterViewModel>
                {
                    new() { Label = "Cars Listed",       Value = "15000", Suffix = "+" },
                    new() { Label = "Happy Customers",   Value = "8200",  Suffix = "+" },
                    new() { Label = "Brands Available",  Value = brands.Count.ToString() },
                    new() { Label = "Years of Service",  Value = "12",    Suffix = "+" }
                }
            };

            return View(vm);
        }
    }
}
