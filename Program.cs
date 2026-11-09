using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PhoneStore.Domain.Repositories;
using PhoneStore.Infrastructure.Data;
using PhoneStore.Infrastructure.Repositories;
using PhoneStore.WebAPI.Middlewares;
using Serilog;
using System.Text;

// =========================================================================
// 1. KHIỂN TẠO SERILOG & BUILDER
// =========================================================================
var builder = WebApplication.CreateBuilder(args);

// Cấu hình đọc Serilog từ appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// =========================================================================
// 2. CẤU HÌNH CÁC DỊCH VỤ (DEPENDENCY INJECTION CONTAINER)
// =========================================================================

// 2.1. Cấu hình DbContext (Kết nối CSDL SQL Server hoặc MySQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2.2. Đăng ký các Repositories (Repository Pattern)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// 2.3. Cấu hình Xác thực JWT (Authentication & Authorization)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "PhoneStoreSuperSecretKey2026StrongEnough!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "PhoneStoreAPI",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "PhoneStoreClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 2.4. Cấu hình CORS (Cho phép Front-end React/Vue/Angular gọi API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

// 2.5. Cấu hình Swagger/OpenAPI (Kèm tích hợp nút nhập Token JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhoneStore API", Version = "v1" });
    
    // Cấu hình Nút "Authorize" trên giao diện Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập chuỗi JWT Token theo định dạng: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =========================================================================
// 3. CẤU HÌNH HTTP REQUEST PIPELINE (MIDDLEWARES)
// =========================================================================
var app = builder.Build();

// 3.1. Đăng ký Global Exception Middleware (Đặt lên ĐẦU PIPELINE để bắt toàn bộ lỗi phát sinh)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3.2. Bật Swagger trong môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhoneStore API v1");
    });
}

app.UseHttpsRedirection();

// 3.3. Kích hoạt CORS Policy
app.UseCors("AllowAllOrigins");

// 3.4. Kích hoạt Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =========================================================================
// 4. KHỞI CHẠY HỆ THỐNG
// =========================================================================
try
{
    Log.Information(">>> Khởi chạy thành công Hệ thống API Bán Điện Thoại...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, ">>> Hệ thống gặp sự cố nghiêm trọng không thể khởi động!");
}
finally
{
    Log.CloseAndFlush();
}
