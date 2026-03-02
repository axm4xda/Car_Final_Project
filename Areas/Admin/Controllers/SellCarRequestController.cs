using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SellCarRequestController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public SellCarRequestController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        // GET: Satış Müraciətləri (Pending + Approved + Rejected)
        public async Task<IActionResult> Index(string? tab = null)
        {
            ViewData["ActivePage"] = "SellCarRequests";

            var requests = await _db.SellCarRequests
                .Where(s => s.Status != SellCarRequestStatus.Trashed)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            ViewBag.CurrentTab = tab ?? "all";
            return View(requests);
        }

        // GET: Zibil Qutusu
        public async Task<IActionResult> Trash()
        {
            ViewData["ActivePage"] = "SellCarTrash";

            var trashed = await _db.SellCarRequests
                .Where(s => s.Status == SellCarRequestStatus.Trashed)
                .OrderByDescending(s => s.TrashedDate)
                .ToListAsync();

            return View(trashed);
        }

        // GET: Detal
        public async Task<IActionResult> Details(int id)
        {
            ViewData["ActivePage"] = "SellCarRequests";
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            // İlk dəfə baxılanda "oxunmuş" kimi işarələ
            if (!req.IsReviewed)
            {
                req.IsReviewed = true;
                await _db.SaveChangesAsync();
            }

            ViewBag.Brands = new SelectList(
                await _db.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name");

            return View(req);
        }

        // POST: Təsdiq et — Car yaradılır, List səhifəsinə düşür
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int brandId, string? adminNote)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            // Yanacaq növü parse
            FuelType fuelType = FuelType.Petrol;
            if (!string.IsNullOrEmpty(req.FuelType))
                Enum.TryParse(req.FuelType, true, out fuelType);

            // Ötürücü parse
            TransmissionType transmission = TransmissionType.Automatic;
            if (!string.IsNullOrEmpty(req.Transmission))
                Enum.TryParse(req.Transmission, true, out transmission);

            // Yeni Car yarat
            var car = new Car
            {
                Title = req.CarTitle,
                Price = req.AskingPrice,
                Year = req.Year,
                Mileage = req.Mileage,
                FuelType = fuelType,
                Transmission = transmission,
                Condition = CarCondition.Used,
                Description = req.Description,
                ThumbnailUrl = req.ImageUrl,
                BrandId = brandId,
                Badge = "New Listing",
                BadgeColor = "bg-primary-2",
                CreatedDate = DateTime.UtcNow,
                IsApproved = true // Admin təsdiqi ilə yaradıldığı üçün
            };

            _db.Cars.Add(car);
            await _db.SaveChangesAsync();

            // Şəkil varsa CarImage olaraq da əlavə et
            if (!string.IsNullOrEmpty(req.ImageUrl))
            {
                _db.CarImages.Add(new CarImage
                {
                    CarId = car.Id,
                    ImageUrl = req.ImageUrl,
                    IsMain = true,
                    Order = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Markanın maşın sayını yenilə
            var brand = await _db.Brands.FindAsync(brandId);
            if (brand != null)
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == brandId && c.IsApproved);

            // Müraciəti təsdiq statusuna keçir
            req.Status = SellCarRequestStatus.Approved;
            req.IsReviewed = true;
            req.AdminNote = adminNote;
            req.ApprovedCarId = car.Id;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Müraciət təsdiqləndi! \"{car.Title}\" adlı maşın List səhifəsinə əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Rədd et — Zibil qutusuna göndər
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? adminNote)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = SellCarRequestStatus.Rejected;
            req.IsReviewed = true;
            req.AdminNote = adminNote;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Müraciət rədd edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Zibil qutusuna at (soft delete)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = SellCarRequestStatus.Trashed;
            req.TrashedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Müraciət zibil qutusuna göndərildi. 10 gün sonra avtomatik silinəcək.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Zibil qutusundan bərpa et
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = SellCarRequestStatus.Pending;
            req.TrashedDate = null;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Müraciət bərpa edildi.";
            return RedirectToAction(nameof(Trash));
        }

        // POST: Həmişəlik sil (permanent delete)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            // Şəkil varsa sil
            if (!string.IsNullOrEmpty(req.ImageUrl))
                _fileService.Delete(req.ImageUrl);

            _db.SellCarRequests.Remove(req);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Müraciət həmişəlik silindi.";
            return RedirectToAction(nameof(Trash));
        }

        // POST: Zibil qutusunu boşalt
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EmptyTrash()
        {
            var trashed = await _db.SellCarRequests
                .Where(s => s.Status == SellCarRequestStatus.Trashed)
                .ToListAsync();

            foreach (var req in trashed)
            {
                if (!string.IsNullOrEmpty(req.ImageUrl))
                    _fileService.Delete(req.ImageUrl);
            }

            _db.SellCarRequests.RemoveRange(trashed);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{trashed.Count} müraciət həmişəlik silindi.";
            return RedirectToAction(nameof(Trash));
        }
    }
}
