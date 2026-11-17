using Backend.API.Models;
using Backend.Domain.Exceptions;

namespace Backend.API.Middleware;

/// <summary>Turns unhandled exceptions into the standard <see cref="ApiResponse"/> shape.</summary>
public class GlobalExceptionMiddleware
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away, so there is nobody to answer.
            _logger.LogDebug("Request {TraceId} was cancelled by the client", context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Request {Method} {Path} failed ({TraceId})",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var (statusCode, message) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                DomainException => (StatusCodes.Status400BadRequest, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, UnexpectedErrorMessage),
            };

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message, context.TraceIdentifier));
        }
    }
}