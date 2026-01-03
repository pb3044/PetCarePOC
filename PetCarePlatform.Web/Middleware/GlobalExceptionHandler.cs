using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using PetCarePlatform.Core.Exceptions;

namespace PetCarePlatform.Web.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var response = httpContext.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = "An error occurred while processing your request.",
            TraceId = httpContext.TraceIdentifier
        };

        switch (exception)
        {
            case EntityNotFoundException ex:
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = ex.Message;
                errorResponse.ErrorCode = "ENTITY_NOT_FOUND";
                _logger.LogWarning(ex, "Entity not found: {EntityName}, ID: {EntityId}", ex.EntityName, ex.EntityId);
                break;

            case ValidationException ex:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = ex.Message;
                errorResponse.ErrorCode = "VALIDATION_ERROR";
                errorResponse.Errors = ex.Errors;
                _logger.LogWarning(ex, "Validation error occurred");
                break;

            case BusinessRuleViolationException ex:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = ex.Message;
                errorResponse.ErrorCode = "BUSINESS_RULE_VIOLATION";
                _logger.LogWarning(ex, "Business rule violation: {RuleName}", ex.RuleName);
                break;

            case UnauthorizedAccessException:
                errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "You are not authorized to perform this action.";
                errorResponse.ErrorCode = "UNAUTHORIZED";
                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;

            default:
                _logger.LogError(exception, "An unhandled exception occurred");
                if (httpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                {
                    errorResponse.Message = exception.Message;
                    errorResponse.StackTrace = exception.StackTrace;
                }
                break;
        }

        response.StatusCode = errorResponse.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsJsonAsync(errorResponse, jsonOptions, cancellationToken);
        return true;
    }

    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
        public string? StackTrace { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}

