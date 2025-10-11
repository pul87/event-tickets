// Api/ExceptionMappingMiddleware.cs
using System.Net;
using System.Text.Json;
using EventTickets.Shared;

public sealed class ExceptionMappingMiddleware : IMiddleware
{
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
    }

    static Task Write(HttpContext ctx, HttpStatusCode code, string message)
    {
        ctx.Response.StatusCode = (int)code;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}

