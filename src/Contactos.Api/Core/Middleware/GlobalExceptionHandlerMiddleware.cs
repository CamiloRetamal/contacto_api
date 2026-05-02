using System.Net;
using System.Text.Json;
using Contactos.Api.Core.Constants;
using Contactos.Api.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Contactos.Api.Core.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var traceId = CorrelationContext.Resolve(context);
        var response = context.Response;
        response.ContentType = "application/problem+json";

        ProblemDetails problem = exception switch
        {
            ContactoNotFoundException nf => LogAndBuildNotFound(nf, traceId, context),
            DuplicateTelefonoException dup => LogAndBuildConflict(dup, traceId, context),
            DomainValidationException dv => LogAndBuildValidation(dv, traceId, context),
            _ => LogAndBuildUnexpected(exception, traceId, context),
        };

        response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        await response.WriteAsync(JsonSerializer.Serialize(problem, jsonOptions));
    }

    private ProblemDetails LogAndBuildNotFound(ContactoNotFoundException ex, string traceId, HttpContext context)
    {
        _logger.LogWarning(ex, "Contact not found. ContactId={ContactId}", ex.Id);
        return new ProblemDetails
        {
            Title = PublicErrorMessages.NotFoundTitle,
            Detail = PublicErrorMessages.NotFoundDetail,
            Status = StatusCodes.Status404NotFound,
            Instance = context.Request.Path.Value ?? string.Empty,
            Extensions = { ["traceId"] = traceId },
        };
    }

    private ProblemDetails LogAndBuildConflict(DuplicateTelefonoException ex, string traceId, HttpContext context)
    {
        _logger.LogWarning(ex, "Duplicate telephone rejected for contact create.");
        return new ProblemDetails
        {
            Title = PublicErrorMessages.ConflictTitle,
            Detail = PublicErrorMessages.ConflictDetail,
            Status = StatusCodes.Status409Conflict,
            Instance = context.Request.Path.Value ?? string.Empty,
            Extensions = { ["traceId"] = traceId },
        };
    }

    private ProblemDetails LogAndBuildValidation(DomainValidationException ex, string traceId, HttpContext context)
    {
        _logger.LogWarning(ex, "Domain validation rejected: {Reason}", ex.Message);
        return new ProblemDetails
        {
            Title = PublicErrorMessages.ValidationTitle,
            Detail = PublicErrorMessages.ValidationDetail,
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path.Value ?? string.Empty,
            Extensions = { ["traceId"] = traceId },
        };
    }

    private ProblemDetails LogAndBuildUnexpected(Exception ex, string traceId, HttpContext context)
    {
        _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        return new ProblemDetails
        {
            Title = PublicErrorMessages.ServerErrorTitle,
            Detail = PublicErrorMessages.ServerErrorDetail,
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path.Value ?? string.Empty,
            Extensions = { ["traceId"] = traceId },
        };
    }
}
