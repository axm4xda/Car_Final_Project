using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.SalesAgent;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class SalesAgentController : Controller
    {
        private readonly ISalesAgentService _salesAgentService;

        public SalesAgentController(ISalesAgentService salesAgentService)
        {
            _salesAgentService = salesAgentService;
        }

        public async Task<IActionResult> Index()
        {
            var agents = await _salesAgentService.GetAllActiveAsync();

            var vm = new SalesAgentIndexViewModel
            {
                Agents = agents.Select(a => new SalesAgentCardViewModel
                {
                    Id           = a.Id,
                    FullName     = a.FullName,
                    Title        = a.Title,
                    ImageUrl     = a.ImageUrl,
                    FacebookUrl  = a.FacebookUrl,
                    TwitterUrl   = a.TwitterUrl,
                    InstagramUrl = a.InstagramUrl,
                    SkypeUrl     = a.SkypeUrl,
                    TelegramUrl  = a.TelegramUrl,
                    Phone1       = a.Phone1,
                    Email        = a.Email
                }).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var agent = await _salesAgentService.GetByIdAsync(id);

            if (agent == null)
                return NotFound();

            var vm = new SalesAgentDetailsViewModel
            {
                Id           = agent.Id,
                FullName     = agent.FullName,
                Title        = agent.Title,
                ImageUrl     = agent.ImageUrl,
                Bio          = agent.Bio,
                Address      = agent.Address,
                Phone1       = agent.Phone1,
                Phone2       = agent.Phone2,
                Email        = agent.Email,
                MapEmbedUrl  = agent.MapEmbedUrl,
                IsVerified   = agent.IsVerified,
                FacebookUrl  = agent.FacebookUrl,
                TwitterUrl   = agent.TwitterUrl,
                InstagramUrl = agent.InstagramUrl,
                SkypeUrl     = agent.SkypeUrl,
                TelegramUrl  = agent.TelegramUrl,
                Reviews      = agent.Reviews.Select(r => new SalesAgentReviewViewModel
                {
                    AuthorName     = r.AuthorName,
                    AuthorAvatarUrl = r.AuthorAvatarUrl,
                    Content        = r.Content,
                    Rating         = r.Rating,
                    CreatedDate    = r.CreatedDate
                }).ToList()
            };

            return View(vm);
        }
    }
}
