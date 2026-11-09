using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhoneStore.WebAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Tiếp tục chuyển request đến các middleware hoặc Controller tiếp theo
                await _next(context);
            }
            catch (Exception ex)
            {
                // 1. Ghi log chi tiết lỗi hệ thống phục vụ công tác kiểm tra (Audit Trail)
                _logger.LogError(ex, "Đã xảy ra lỗi ngoại lệ chưa xử lý: {Message}", ex.Message);

                // 2. Xử lý phản hồi lỗi trả về cho Client
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Đóng gói thông tin lỗi thân thiện, bảo mật (che giấu StackTrace kỹ thuật)
            var errorResponse = new
            {
                success = false,
                statusCode = context.Response.StatusCode,
                message = "Đã xảy ra lỗi hệ thống nội bộ. Vui lòng thử lại sau.",
                detailed = exception.Message // Trong môi trường Production có thể ẩn trường này đi
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
