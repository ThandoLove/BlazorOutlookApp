using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using OperationalWorkspaceApplication.Exceptions;

namespace OperationalWorkspaceAPI.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        _logger.LogError(exception, "[TraceId: {TraceId}] Global API handler caught uncaught exception layer fail.", traceId);

        context.Response.ContentType = "application/problem+json";

        // Dynamically assign correct HTTP status codes to custom use case errors
        var status = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            CustomerNotFoundException => StatusCodes.Status404NotFound,
            SageAuthenticationException => StatusCodes.Status401Unauthorized,
            GraphQLQueryException => StatusCodes.Status502BadGateway,
            DocumentAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = status;

        var problemDetails = new
        {
            Type = $"https://workspace.local{exception.GetType().Name.ToLowerInvariant()}",
            Title = exception switch
            {
                ArgumentException => "Parameter Validation Error",
                CustomerNotFoundException => "Sage Account Missing",
                _ => "API Transaction Process Failure"
            },
            Status = status,
            Detail = exception.Message,
            Instance = context.Request.Path.Value,
            TraceId = traceId
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}
