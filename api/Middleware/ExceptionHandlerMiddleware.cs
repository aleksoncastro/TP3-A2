using MediaMatch.Exceptions;
using System.Net;
using System.Text.Json;

namespace MediaMatch.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção não tratada: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
                ForbiddenException forbidden => (HttpStatusCode.Forbidden, forbidden.Message),
                BusinessException business => (HttpStatusCode.BadRequest, business.Message),
                ValidationException validation => (HttpStatusCode.BadRequest, validation.Message),
                UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, unauthorized.Message),
                UnauthorizedAccessException unauthorized => (HttpStatusCode.Unauthorized, unauthorized.Message),
                _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                error = message,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsJsonAsync(response, jsonOptions);
        }
    }
}
