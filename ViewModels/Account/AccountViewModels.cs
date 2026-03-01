using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad mütl?qdir")]
        [StringLength(100, ErrorMessage = "Ad Soyad maksimum 100 simvol ola bil?r")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email mütl?qdir")]
        [EmailAddress(ErrorMessage = "Düzgün email format? daxil edin")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "?ifr? mütl?qdir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "?ifr? minimum 8 simvol olmal?d?r")]
        [DataType(DataType.Password)]
        [Display(Name = "?ifr?")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "?ifr?ni t?krarlamaq mütl?qdir")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "?ifr?l?r uy?un g?lmir")]
        [Display(Name = "?ifr?ni T?krarla")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "??rtl?ri q?bul etm?lisiniz")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "?stifad?çi ??rtl?rini q?bul etm?lisiniz")]
        public bool AgreeToTerms { get; set; }

        [Required(ErrorMessage = "Rol seçimi mütl?qdir")]
        [Display(Name = "Rol")]
        public string Role { get; set; } = "User";
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email mütl?qdir")]
        [EmailAddress(ErrorMessage = "Düzgün email format? daxil edin")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "?ifr? mütl?qdir")]
        [DataType(DataType.Password)]
        [Display(Name = "?ifr?")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "M?ni Xat?rla")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email mütl?qdir")]
        [EmailAddress(ErrorMessage = "Düzgün email format? daxil edin")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public class UserWithRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedDate { get; set; }
    }
}
