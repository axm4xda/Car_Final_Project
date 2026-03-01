using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class NewsletterSubscriberService : INewsletterSubscriberService
    {
        private readonly ApplicationDbContext _context;

        public NewsletterSubscriberService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ?? PUBLIC ????????????????????????????????????????????????????????????

        public async Task SubscribeAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("E-poçt ünvan? bo? ola bilm?z.", nameof(email));

            var existing = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Email == email);

            if (existing != null)
            {
                // Art?q mövcuddursa deaktiv olmu? ola bil?r — yenid?n aktiv et
                existing.IsActive = true;
            }
            else
            {
                await _context.NewsletterSubscribers.AddAsync(new NewsletterSubscriber
                {
                    Email       = email,
                    IsActive    = true,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UnsubscribeAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("E-poçt ünvan? bo? ola bilm?z.", nameof(email));

            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Email == email);

            if (subscriber == null) return; // Tap?lmasa sessizce ç?x

            subscriber.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsSubscribedAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            return await _context.NewsletterSubscribers
                .AnyAsync(n => n.Email == email && n.IsActive);
        }

        // ?? ADMIN ?????????????????????????????????????????????????????????????

        public async Task<IList<NewsletterSubscriber>> GetAllAdminAsync()
        {
            return await _context.NewsletterSubscribers
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IList<NewsletterSubscriber>> GetActiveAsync()
        {
            return await _context.NewsletterSubscribers
                .AsNoTracking()
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.NewsletterSubscribers
                .CountAsync(n => n.IsActive);
        }

        public async Task DeleteAsync(int id)
        {
            var subscriber = await _context.NewsletterSubscribers.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan abun?çi tap?lmad?.");

            _context.NewsletterSubscribers.Remove(subscriber);
            await _context.SaveChangesAsync();
        }
    }
}
