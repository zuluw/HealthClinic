using InnoClinic.Offices.Domain.Exceptions;
using System.Net;

namespace InnoClinic.Offices.Api.Middleware
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
                logger.LogError(ex, "Unhandled exception occurred at {Path}: {Message}", context.Request.Path, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            if (exception is BaseException baseEx)
            {
                context.Response.StatusCode = baseEx.StatusCode;
                var response = new ErrorResponse(baseEx.StatusCode, baseEx.Message);
                return context.Response.WriteAsJsonAsync(response);
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsJsonAsync(new ErrorResponse(500, "Internal Server Error!"));
        }
    }
}