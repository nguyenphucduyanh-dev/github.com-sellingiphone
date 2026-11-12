using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Models;

namespace PhoneStore.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Tiêm trực tiếp DbContext để thao tác CSDL nhanh chóng
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Product/Index
        // Hỗ trợ đồng thời: Tìm kiếm từ khóa, Lọc theo Hãng (Category), Lọc theo khoảng Giá
        public async Task<IActionResult> Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            // Khởi tạo truy vấn ban đầu kèm theo bảng Category (Hãng điện thoại)
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // 1. Lọc theo từ khóa tìm kiếm (Tên điện thoại hoặc mô tả)
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
                
                // Giữ lại từ khóa tìm kiếm trên giao diện View để hiển thị lại cho người dùng
                ViewBag.SearchString = searchString;
            }

            // 2. Lọc theo hãng sản xuất (CategoryId)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                productsQuery = productsQuery.where(p => p.CategoryId == categoryId.Value);
                ViewBag.SelectedCategoryId = categoryId.Value;
            }

            // 3. Lọc theo khoảng giá (Từ giá min đến giá max)
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.where(p => p.Price >= minPrice.Value);
                ViewBag.MinPrice = minPrice.Value;
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.where(p => p.Price <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice.Value;
            }

            // Truyền danh sách Hãng (Categories) sang giao diện để đổ dữ liệu vào thẻ <select> hoặc menu lọc
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // Thực thi câu lệnh truy vấn SQL lấy danh sách sản phẩm thỏa mãn điều kiện
            var model = await productsQuery.ToListAsync();

            return View(model);
        }
    }
}
