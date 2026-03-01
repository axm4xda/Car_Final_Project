using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Car_Project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ?? MVC ???????????????????????????????????????????????????????????
            builder.Services.AddControllersWithViews();

            // ?? Database ??????????????????????????????????????????????????????
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // ?? Identity ????????????????????????????????????????????????????
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // ?ifr? qaydalar?
                options.Password.RequireDigit           = true;
                options.Password.RequiredLength         = 8;
                options.Password.RequireUppercase       = true;
                options.Password.RequireLowercase       = true;
                options.Password.RequireNonAlphanumeric = false;

                // Email unikall???
                options.User.RequireUniqueEmail = true;

                // Lockout
                options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers      = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ?? Cookie ??????????????????????????????????????????????????????
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath        = "/";
                options.LogoutPath       = "/Account/Logout";
                options.AccessDeniedPath = "/";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan    = TimeSpan.FromDays(7);
            });

            // ?? Session (CompareItem üçün) ????????????????????????????????????
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout        = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly    = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddHttpContextAccessor();

            // ?? Services ??????????????????????????????????????????????????????
            builder.Services.AddScoped<IFileService,                 FileService>();
            builder.Services.AddScoped<ICarService,                  CarService>();
            builder.Services.AddScoped<IBrandService,                BrandService>();
            builder.Services.AddScoped<ICarImageService,             CarImageService>();
            builder.Services.AddScoped<ICarFeatureService,           CarFeatureService>();
            builder.Services.AddScoped<IReviewService,               ReviewService>();
            builder.Services.AddScoped<IFAQService,                  FAQService>();
            builder.Services.AddScoped<IContactMessageService,       ContactMessageService>();
            builder.Services.AddScoped<ILoanCalculationService,      LoanCalculationService>();
            builder.Services.AddScoped<ISellCarRequestService,       SellCarRequestService>();
            builder.Services.AddScoped<IServiceCenterService,        ServiceCenterService>();
            builder.Services.AddScoped<INewsletterSubscriberService, NewsletterSubscriberService>();
            builder.Services.AddScoped<ICompareItemService,          CompareItemService>();
            builder.Services.AddScoped<IWishlistService,             WishlistService>();
            // Blog
            builder.Services.AddScoped<IBlogService,     BlogService>();
            // Shop
            builder.Services.AddScoped<IProductService,  ProductService>();
            builder.Services.AddScoped<ICartService,     CartService>();
            // Checkout / Payment
            builder.Services.AddScoped<IOrderService,    OrderService>();
            builder.Services.AddScoped<IPaymentService,  PaymentService>();
            builder.Services.AddScoped<ICouponService,   CouponService>();
            // SalesAgent
            builder.Services.AddScoped<ISalesAgentService, SalesAgentService>();

            // ?? Build ?????????????????????????????????????????????????????????
            var app = builder.Build();

            // ?? Roles Seed ????????????????????????????????????????????????????
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                // Rollar? yarat
                foreach (var role in new[] { "SuperAdmin", "Admin", "Agent", "User" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // SuperAdmin istifad?çi yarat (?g?r yoxdursa)
                var superAdminEmail = "superadmin@aurexo.com";
                var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
                if (superAdmin == null)
                {
                    superAdmin = new AppUser
                    {
                        FullName    = "Super Admin",
                        Email       = superAdminEmail,
                        UserName    = superAdminEmail,
                        CreatedDate = DateTime.UtcNow,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(superAdmin, "SuperAdmin123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                    }
                }
                else if (!await userManager.IsInRoleAsync(superAdmin, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
