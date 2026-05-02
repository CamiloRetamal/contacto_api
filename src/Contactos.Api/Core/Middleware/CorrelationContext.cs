using Microsoft.AspNetCore.Http;

namespace Contactos.Api.Core.Middleware;

public static class CorrelationContext
{
    public const string HeaderName = "X-Correlation-Id";
    public const string HttpContextItemKey = "CorrelationId";

    public static string Resolve(HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(HttpContextItemKey, out var value) && value is string s && !string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        return httpContext.TraceIdentifier;
    }
}
