using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Helpers;
using PhoneStore.Web.Models;

namespace PhoneStore.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "CartSessionKey";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách giỏ hàng hiện tại từ Session
        private List<CartItem> GetCartItems()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartSessionKey);
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            return cart;
        }

        // Lưu lại giỏ hàng vào Session
        private void SaveCartSession(List<CartItem> cart)
        {
            HttpContext.Session.Set(CartSessionKey, cart);
        }

        // GET: /Cart/Index (Xem giỏ hàng)
        public IActionResult Index()
        {
            var cart = GetCartItems();
            ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(cart);
        }

        // POST: /Cart/AddToCart (Thêm sản phẩm vào giỏ)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(p => p.ProductId == productId);

            if (cartItem == null)
            {
                // Nếu sản phẩm chưa có trong giỏ, thêm mới
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }
            else
            {
                // Nếu có rồi, tăng số lượng lên
                cartItem.Quantity += quantity;
            }

            SaveCartSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Remove/5 (Xóa sản phẩm khỏi giỏ)
        public IActionResult Remove(int id)
        {
            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(p => p.ProductId == id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCartSession(cart);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
