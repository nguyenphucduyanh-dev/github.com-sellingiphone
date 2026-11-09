using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Middlewares;
using PhoneStore.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối CSDL trực tiếp qua Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Sử dụng Cookie Authentication (Đăng nhập đơn giản, không cần Token phức tạp)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";     // Đường dẫn nếu chưa đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Không đủ quyền
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. Đăng ký Middleware xử lý lỗi toàn cục (Bắt lỗi văng màn hình)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Cho phép load file CSS, JS, ảnh trong wwwroot

app.UseRouting();

// 4. Kích hoạt phân quyền đăng nhập
app.UseAuthentication();
app.UseAuthorization();

// 5. Định nghĩa Route mặc định cho Website MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
