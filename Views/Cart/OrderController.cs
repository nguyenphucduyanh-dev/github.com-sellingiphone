using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneStore.Web.Helpers;
using PhoneStore.Web.Models;
using System.Security.Claims;

namespace PhoneStore.Web.Controllers
{
    // Bắt buộc khách hàng phải đăng nhập mới được tiến hành đặt hàng
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "CartSessionKey";

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Order/Checkout (Hiển thị trang nhập thông tin giao hàng)
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey);
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = cart.Sum(x => x.TotalPrice);
            return View();
        }

        // POST: /Order/Checkout (Xử lý lưu đơn hàng & Trừ tồn kho)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string receiverName, string receiverPhone, string shippingAddress, string? note)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey);
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Lấy UserId của tài khoản đang đăng nhập từ Cookie Claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim!);

            // Bắt đầu Transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo mới đối tượng Đơn hàng (Orders)
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    ReceiverName = receiverName,
                    ReceiverPhone = receiverPhone,
                    ShippingAddress = shippingAddress,
                    Note = note,
                    TotalAmount = cart.Sum(x => x.TotalPrice),
                    Status = 0 // 0: Chờ xử lý, 1: Đang giao, 2: Hoàn thành, -1: Đã hủy
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu trước để lấy được Order.Id tự tăng

                // 2. Lưu Chi tiết đơn hàng (OrderDetails) và Trừ tồn kho sản phẩm
                foreach (var item in cart)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    
                    // Kiểm tra tồn kho lần cuối trước khi chốt đơn
                    if (product == null || product.Stock < item.Quantity)
                    {
                        throw new Exception($"Sản phẩm '{item.ProductName}' hiện không đủ số lượng trong kho.");
                    }

                    // Trừ số lượng tồn kho
                    product.Stock -= item.Quantity;
                    _context.Products.Update(product);

                    // Thêm chi tiết đơn hàng
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                }

                await _context.SaveChangesAsync();

                // 3. Xác nhận hoàn tất Transaction
                await transaction.CommitAsync();

                // 4. Xóa sạch giỏ hàng trong Session sau khi đặt thành công
                HttpContext.Session.Remove(CartSessionKey);

                return RedirectToAction(nameof(OrderSuccess), new { id = order.Id });
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi nào (VD: hết hàng giữa chừng), hoàn tác toàn bộ thao tác CSDL
                await transaction.RollbackAsync();
                
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Checkout));
            }
        }

        // GET: /Order/OrderSuccess/5 (Trang thông báo đặt hàng thành công)
        public IActionResult OrderSuccess(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}
