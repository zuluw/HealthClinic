using InnoClinic.Notifications.Domain.Exceptions;
using System.Net;

namespace InnoClinic.Notifications.Api.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        private record ErrorResponse(int StatusCode, string Message);

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            if (exception is BaseException baseEx)
            {
                context.Response.StatusCode = baseEx.StatusCode;
                return context.Response.WriteAsJsonAsync(new ErrorResponse(baseEx.StatusCode, baseEx.Message));
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsJsonAsync(new ErrorResponse(500, "Internal Server Error!"));
        }
    }
}