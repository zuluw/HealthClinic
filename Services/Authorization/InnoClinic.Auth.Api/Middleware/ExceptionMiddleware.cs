using InnoClinic.Auth.Api.Models;
using InnoClinic.Auth.Domain.Exceptions;
using System.Net;

namespace InnoClinic.Auth.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred.");

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