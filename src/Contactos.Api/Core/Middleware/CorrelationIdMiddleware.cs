using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Contactos.Api.Core.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationContext.HeaderName, out var fromHeader)
            && !string.IsNullOrWhiteSpace(fromHeader)
            ? fromHeader.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[CorrelationContext.HttpContextItemKey] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationContext.HeaderName, correlationId);
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
