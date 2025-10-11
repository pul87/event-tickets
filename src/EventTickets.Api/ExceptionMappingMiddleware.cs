// Api/ExceptionMappingMiddleware.cs
using System.Net;
using System.Text.Json;
using EventTickets.Shared;
using Microsoft.Extensions.Logging;

public sealed class ExceptionMappingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMappingMiddleware> _logger;
    public ExceptionMappingMiddleware(ILogger<ExceptionMappingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (NotFoundException ex)
        {
            await Write(ctx, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConcurrencyException ex)
        {
            await Write(ctx, HttpStatusCode.Conflict, ex.Message);
        }
        catch (DomainException ex)
        {
            await Write(ctx, (HttpStatusCode)422, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await Write(ctx, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await Write(ctx, HttpStatusCode.InternalServerError, "Unexpected error");
        }
    }

    static Task Write(HttpContext ctx, HttpStatusCode code, string message)
    {
        ctx.Response.StatusCode = (int)code;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}

