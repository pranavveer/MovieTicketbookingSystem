using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.Exceptions;

namespace MovieTicketBooking.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception: {Message}", exception.Message);

        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Business Conflict"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var problem = new ProblemDetails
        {
            Status = status, Title = title, Instance = httpContext.Request.Path,
            Detail = status == 500 ? "An unexpected error occurred." : exception.Message
        };

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
