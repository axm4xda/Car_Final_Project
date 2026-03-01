using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService    _cartService;
        private readonly IOrderService   _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ICouponService  _couponService;

        public CheckoutController(
            ICartService    cartService,
            IOrderService   orderService,
            IPaymentService paymentService,
            ICouponService  couponService)
        {
            _cartService    = cartService;
            _orderService   = orderService;
            _paymentService = paymentService;
            _couponService  = couponService;
        }

        // GET /Checkout
        public async Task<IActionResult> Index()
        {
            var sessionId = HttpContext.Session.Id;
            var cartItems = await _cartService.GetCartAsync(sessionId);

            var summaryItems = cartItems.Select(ci => new CheckoutOrderItemViewModel
            {
                ProductName  = ci.Product.Name,
                ThumbnailUrl = ci.Product.ThumbnailUrl
                               ?? ci.Product.Images.FirstOrDefault()?.ImageUrl,
                Quantity     = ci.Quantity,
                UnitPrice    = ci.Product.Price
            }).ToList();

            var subTotal = summaryItems.Sum(i => i.TotalPrice);

            var vm = new CheckoutPageViewModel
            {
                Form    = new CheckoutFormViewModel(),
                Summary = new CheckoutOrderSummaryViewModel
                {
                    Items        = summaryItems,
                    SubTotal     = subTotal,
                    ShippingCost = 0,
                    Discount     = 0,
                    Total        = subTotal
                }
            };

            return View(vm);
        }

        // POST /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutPageViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Xülasəni yenidən doldur
                var sessionIdErr = HttpContext.Session.Id;
                var cartItemsErr = await _cartService.GetCartAsync(sessionIdErr);
                vm.Summary.Items = cartItemsErr.Select(ci => new CheckoutOrderItemViewModel
                {
                    ProductName  = ci.Product.Name,
                    ThumbnailUrl = ci.Product.ThumbnailUrl,
                    Quantity     = ci.Quantity,
                    UnitPrice    = ci.Product.Price
                }).ToList();
                vm.Summary.SubTotal = vm.Summary.Items.Sum(i => i.TotalPrice);
                vm.Summary.Total    = vm.Summary.SubTotal - vm.Summary.Discount + vm.Summary.ShippingCost;
                return View("Index", vm);
            }

            var sessionId = HttpContext.Session.Id;
            var form      = vm.Form;

            // Kupon yoxlaması
            decimal discount = 0;
            if (!string.IsNullOrWhiteSpace(form.CouponCode))
            {
                var cartItems   = await _cartService.GetCartAsync(sessionId);
                var orderTotal  = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
                var coupon      = await _couponService.ValidateAsync(form.CouponCode, orderTotal);
                if (coupon != null)
                {
                    discount = coupon.DiscountAmount.HasValue
                        ? coupon.DiscountAmount.Value
                        : Math.Round(orderTotal * coupon.DiscountPercent / 100, 2);
                    await _couponService.UseAsync(form.CouponCode);
                }
            }

            // Ödəniş obyekti yarat
            var payment = new Payment
            {
                Method         = form.PaymentMethod,
                CardHolderName = form.CardHolderName,
                CardLastFour   = form.CardNumber?.Length >= 4
                                    ? form.CardNumber[^4..]
                                    : form.CardNumber,
                Amount         = 0   // Order yaradıldıqdan sonra doldurulacaq
            };

            var processedPayment = await _paymentService.ProcessAsync(payment);

            // Sifariş yarat
            var order = new Order
            {
                FirstName    = form.FirstName,
                LastName     = form.LastName,
                Email        = form.Email,
                Phone        = form.Phone,
                Country      = form.Country,
                City         = form.City,
                Street       = form.Street,
                State        = form.State,
                PostalCode   = form.PostalCode,
                Note         = form.Note,
                Discount     = discount,
                ShippingCost = 0,
                CouponCode   = form.CouponCode,
                PaymentId    = processedPayment.Id
            };

            var placedOrder = await _orderService.PlaceOrderAsync(order, sessionId);

            // Ödəniş məbləğini yenilə
            processedPayment.Amount = placedOrder.Total;

            return RedirectToAction("Success", new { orderId = placedOrder.Id });
        }

        // POST /Checkout/ApplyCoupon  (AJAX üçün)
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            var sessionId  = HttpContext.Session.Id;
            var cartItems  = await _cartService.GetCartAsync(sessionId);
            var orderTotal = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);

            var coupon = await _couponService.ValidateAsync(couponCode, orderTotal);
            if (coupon == null)
                return Json(new { success = false, message = "Kupon etibarsız və ya müddəti bitib." });

            var discountAmount = coupon.DiscountAmount.HasValue
                ? coupon.DiscountAmount.Value
                : Math.Round(orderTotal * coupon.DiscountPercent / 100, 2);

            return Json(new
            {
                success        = true,
                discountAmount,
                discountLabel  = coupon.DiscountAmount.HasValue
                                    ? $"${discountAmount:F2}"
                                    : $"%{coupon.DiscountPercent}"
            });
        }

        // GET /Checkout/Success?orderId=5
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null) return RedirectToAction("Index", "Home");
            return View(order);
        }
    }
}
