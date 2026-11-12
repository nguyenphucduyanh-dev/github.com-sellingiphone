using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Models;

namespace PhoneStore.Web.Controllers
{
    // Bắt buộc phải có quyền Admin mới được vào khu vực này
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminOrder/Index (Danh sách tất cả đơn hàng, có lọc theo trạng thái)
        public async Task<IActionResult> Index(int? status)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate) // Đơn mới nhất đưa lên đầu
                .AsQueryable();

            if (status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status.Value);
                ViewBag.SelectedStatus = status.Value;
            }

            return View(await ordersQuery.ToListAsync());
        }

        // GET: /AdminOrder/Details/5 (Xem chi tiết các máy điện thoại trong 1 đơn hàng)
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product) // Join lấy tên và ảnh điện thoại
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /AdminOrder/UpdateStatus (Cập nhật trạng thái đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, int newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            // Quy ước trạng thái:
            // 0: Chờ xử lý | 1: Đang giao hàng | 2: Hoàn thành | -1: Đã hủy

            // Nghiệp vụ quan trọng: Nếu Admin HỦY đơn (-1) từ trạng thái chưa hủy, phải HOÀN LẠI TỒN KHO
            if (newStatus == -1 && order.Status != -1)
            {
                foreach (var item in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity; // Cộng trả lại kho
                        _context.Products.Update(product);
                    }
                }
            }

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} thành công!";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
