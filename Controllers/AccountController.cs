using Car_Project.Models;
using Car_Project.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _roleManager   = roleManager;
        }

        private bool IsAjax() =>
            Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        private string GetReturnUrl(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Home")!;

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            var redirect = GetReturnUrl(returnUrl);

            // AgreeToTerms checkbox-? browser gönd?rmir (unchecked), manual yoxla
            if (!model.AgreeToTerms)
            {
                var msg = "?stifad?çi ??rtl?rini q?bul etm?lisiniz.";
                if (IsAjax()) return Json(new { success = false, message = msg });
                TempData["AuthError"] = msg;
                TempData["OpenModal"] = "SignUpModal";
                return Redirect(redirect);
            }

            // Rol yoxla - yaln?z Agent v? User icaz?li
            var allowedRoles = new[] { "Agent", "User" };
            if (string.IsNullOrEmpty(model.Role) || !allowedRoles.Contains(model.Role))
                model.Role = "User";

            // AgreeToTerms x?tas?n? ModelState-d?n ç?xar, çünki yuxar?da yoxlad?q
            ModelState.Remove(nameof(model.AgreeToTerms));

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "M?lumatlar? düzgün daxil edin.";
                if (IsAjax()) return Json(new { success = false, message = firstError });
                TempData["AuthError"] = firstError;
                TempData["OpenModal"] = "SignUpModal";
                return Redirect(redirect);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                var msg = "Bu email art?q qeyd? al?nm??d?r.";
                if (IsAjax()) return Json(new { success = false, message = msg });
                TempData["AuthError"] = msg;
                TempData["OpenModal"] = "SignUpModal";
                return Redirect(redirect);
            }

            var user = new AppUser
            {
                FullName    = model.FullName,
                Email       = model.Email,
                UserName    = model.Email,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);
                var successMsg = $"Xo? g?ldiniz, {user.FullName}! Qeydiyyat u?urlu oldu.";
                if (IsAjax()) return Json(new { success = true, message = successMsg, redirectUrl = redirect });
                TempData["AuthSuccess"] = successMsg;
                return Redirect(redirect);
            }

            // Identity x?talar?n? Az?rbaycan dilin? çevir
            var errors = string.Join(" ", result.Errors.Select(e => TranslateIdentityError(e)));
            if (IsAjax()) return Json(new { success = false, message = errors });
            TempData["AuthError"] = errors;
            TempData["OpenModal"] = "SignUpModal";
            return Redirect(redirect);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            var redirect = GetReturnUrl(returnUrl);

            if (!ModelState.IsValid)
            {
                var msg = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                if (IsAjax()) return Json(new { success = false, message = msg });
                TempData["AuthError"] = msg;
                TempData["OpenModal"] = "LoginModal";
                return Redirect(redirect);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var successMsg = $"Xo? g?ldiniz, {user?.FullName ?? model.Email}!";
                if (IsAjax()) return Json(new { success = true, message = successMsg, redirectUrl = redirect });
                TempData["AuthSuccess"] = successMsg;
                return Redirect(redirect);
            }

            string errorMsg;
            if (result.IsLockedOut)
            {
                errorMsg = "Hesab?n?z müv?qq?ti olaraq bloklan?b. Bir az sonra yenid?n c?hd edin.";
            }
            else
            {
                errorMsg = "Email v? ya ?ifr? yanl??d?r.";
            }

            if (IsAjax()) return Json(new { success = false, message = errorMsg });
            TempData["AuthError"] = errorMsg;
            TempData["OpenModal"] = "LoginModal";
            return Redirect(redirect);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["AuthSuccess"] = "U?urla ç?x?? etdiniz.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "User";

            return View(user);
        }

        // ========== SuperAdmin: ?stifad?çi ?dar?etm? ==========

        // GET: /Account/ManageUsers
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            var userList = new List<UserWithRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserWithRoleViewModel
                {
                    Id        = user.Id,
                    FullName  = user.FullName,
                    Email     = user.Email ?? "",
                    Role      = roles.FirstOrDefault() ?? "User",
                    CreatedDate = user.CreatedDate
                });
            }

            return View(userList);
        }

        // POST: /Account/ChangeUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var allowedRoles = new[] { "Admin", "Agent", "User" };
            if (!allowedRoles.Contains(newRole))
            {
                TempData["AuthError"] = "Yanl?? rol seçimi.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AuthError"] = "?stifad?çi tap?lmad?.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // SuperAdmin rolunu d?yi?m?k olmaz
            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                TempData["AuthError"] = "SuperAdmin-in rolunu d?yi?m?k mümkün deyil.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Köhn? rollar? sil
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Yeni rol t?yin et
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["AuthSuccess"] = $"{user.FullName} istifad?çisin? \"{newRole}\" rolu t?yin edildi.";
            return RedirectToAction(nameof(ManageUsers));
        }

        // POST: /Account/RemoveUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AuthError"] = "?stifad?çi tap?lmad?.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // SuperAdmin-i silm?k olmaz
            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                TempData["AuthError"] = "SuperAdmin hesab?n? silm?k mümkün deyil.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["AuthSuccess"] = $"{user.FullName} istifad?çisi silindi.";
            }
            else
            {
                TempData["AuthError"] = "?stifad?çini silm?k mümkün olmad?.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        /// <summary>
        /// Identity x?ta mesajlar?n? Az?rbaycan dilin? çevirir.
        /// </summary>
        private static string TranslateIdentityError(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateEmail"          => "Bu email art?q qeydiyyatdan keçib.",
                "DuplicateUserName"       => "Bu istifad?çi ad? art?q mövcuddur.",
                "InvalidEmail"            => "Düzgün email format? daxil edin.",
                "InvalidUserName"         => "?stifad?çi ad? düzgün deyil.",
                "PasswordTooShort"        => "?ifr? minimum 8 simvol olmal?d?r.",
                "PasswordRequiresDigit"   => "?ifr?d? ?n az? bir r?q?m olmal?d?r.",
                "PasswordRequiresLower"   => "?ifr?d? ?n az? bir kiçik h?rf olmal?d?r.",
                "PasswordRequiresUpper"   => "?ifr?d? ?n az? bir böyük h?rf olmal?d?r.",
                "PasswordRequiresNonAlphanumeric" => "?ifr?d? ?n az? bir xüsusi simvol olmal?d?r.",
                "PasswordRequiresUniqueChars"     => "?ifr?d? daha çox f?rqli simvol olmal?d?r.",
                _                         => error.Description
            };
        }
    }
}
