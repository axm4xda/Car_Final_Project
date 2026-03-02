using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Blog;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IActionResult> Index(
            string? category = null,
            string? tag = null,
            string? q = null,
            int page = 1)
        {
            const int pageSize = 9;

            var (posts, totalCount) = await _blogService.GetPublishedAsync(
                page, pageSize, category, tag, q);

            var categories  = await _blogService.GetCategoriesAsync();
            var tags        = await _blogService.GetTagsAsync();
            var recentPosts = await _blogService.GetRecentAsync(5);

            var vm = new BlogIndexViewModel
            {
                Posts = posts.Select(p => new BlogPostCardViewModel
                {
                    Id              = p.Id,
                    Title           = p.Title,
                    Slug            = p.Slug,
                    Summary         = p.Summary,
                    ThumbnailUrl    = p.ThumbnailUrl,
                    AuthorName      = p.AuthorName,
                    AuthorAvatarUrl = p.AuthorAvatarUrl,
                    CategoryName    = p.Category?.Name ?? string.Empty,
                    PublishedAt     = p.PublishedAt,
                    ViewCount       = p.ViewCount,
                    Tags            = p.PostTags.Select(pt => pt.BlogTag.Name).ToList()
                }).ToList(),

                Categories    = categories.Select(c => c.Name).ToList(),
                PopularTags   = tags.Select(t => t.Name).ToList(),

                RecentPosts = recentPosts.Select(p => new BlogPostCardViewModel
                {
                    Id           = p.Id,
                    Title        = p.Title,
                    Slug         = p.Slug,
                    ThumbnailUrl = p.ThumbnailUrl,
                    PublishedAt  = p.PublishedAt,
                    CategoryName = p.Category?.Name ?? string.Empty
                }).ToList(),

                SelectedCategory = category,
                SelectedTag      = tag,
                SearchQuery      = q,
                CurrentPage      = page,
                TotalPages       = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalCount       = totalCount
            };

            return View(vm);
        }

        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var post = await _blogService.GetBySlugAsync(slug);
            if (post == null)
                return NotFound();

            await _blogService.IncrementViewCountAsync(post.Id);

            var relatedPosts = await _blogService.GetRelatedAsync(post.CategoryId, post.Id, 3);
            var categories = await _blogService.GetCategoriesAsync();
            var tags = await _blogService.GetTagsAsync();
            var recentPosts = await _blogService.GetRecentAsync(4);
            var (prevPost, nextPost) = await _blogService.GetAdjacentPostsAsync(post.Id);

            var vm = new BlogDetailViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                ThumbnailUrl = post.ThumbnailUrl,
                AuthorName = post.AuthorName,
                AuthorAvatarUrl = post.AuthorAvatarUrl,
                CategoryName = post.Category?.Name ?? "",
                PublishedAt = post.PublishedAt,
                ViewCount = post.ViewCount,
                Tags = post.PostTags.Select(pt => pt.BlogTag.Name).ToList(),
                Comments = post.Comments
                    .Where(c => c.IsApproved)
                    .OrderByDescending(c => c.CreatedDate)
                    .Select(c => new BlogCommentViewModel
                    {
                        Id = c.Id,
                        AuthorName = c.AuthorName,
                        Content = c.Content,
                        CreatedDate = c.CreatedDate
                    }).ToList(),
                CommentForm = new BlogCommentFormViewModel { BlogPostId = post.Id },
                RelatedPosts = relatedPosts.Select(p => new BlogPostCardViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    ThumbnailUrl = p.ThumbnailUrl,
                    AuthorName = p.AuthorName,
                    PublishedAt = p.PublishedAt,
                    CategoryName = p.Category?.Name ?? ""
                }).ToList(),
                PrevPost = prevPost == null ? null : new BlogPostCardViewModel
                {
                    Id = prevPost.Id,
                    Title = prevPost.Title,
                    Slug = prevPost.Slug,
                    ThumbnailUrl = prevPost.ThumbnailUrl,
                    CategoryName = prevPost.Category?.Name ?? "",
                    PublishedAt = prevPost.PublishedAt
                },
                NextPost = nextPost == null ? null : new BlogPostCardViewModel
                {
                    Id = nextPost.Id,
                    Title = nextPost.Title,
                    Slug = nextPost.Slug,
                    ThumbnailUrl = nextPost.ThumbnailUrl,
                    CategoryName = nextPost.Category?.Name ?? "",
                    PublishedAt = nextPost.PublishedAt
                }
            };

            ViewBag.Categories = categories;
            ViewBag.AllTags = tags;
            ViewBag.RecentPosts = recentPosts;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(BlogCommentFormViewModel form)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Detail), new { slug = form.BlogPostId });

            var post = await _blogService.GetByIdAsync(form.BlogPostId);
            if (post == null)
                return NotFound();

            await _blogService.AddCommentAsync(new Models.BlogComment
            {
                BlogPostId = form.BlogPostId,
                AuthorName = form.AuthorName,
                AuthorEmail = form.AuthorEmail,
                Content = form.Content
            });

            return RedirectToAction(nameof(Detail), new { slug = post.Slug });
        }
    }
}
