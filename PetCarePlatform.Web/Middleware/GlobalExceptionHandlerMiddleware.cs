using System.Net;
using System.Text.Json;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.Exceptions;

namespace PetCarePlatform.Web.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
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
                _logger.LogError(ex, "An unhandled exception occurred. {ExceptionType}: {Message}", 
                    ex.GetType().Name, ex.Message);
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An error occurred while processing your request.",
                ErrorCode = Constants.ErrorCodes.InternalError
            };

            switch (exception)
            {
                case EntityNotFoundException ex:
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = ex.Message;
                    errorResponse.ErrorCode = Constants.ErrorCodes.EntityNotFound;
                    break;

                case ValidationException ex:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = ex.Message;
                    errorResponse.ErrorCode = Constants.ErrorCodes.ValidationError;
                    errorResponse.Errors = ex.Errors;
                    break;

                case BusinessRuleViolationException ex:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = ex.Message;
                    errorResponse.ErrorCode = Constants.ErrorCodes.BusinessRuleViolation;
                    break;

                case UnauthorizedAccessException:
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "You are not authorized to perform this action.";
                    errorResponse.ErrorCode = Constants.ErrorCodes.Unauthorized;
                    break;

                case ArgumentException ex:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = ex.Message;
                    errorResponse.ErrorCode = Constants.ErrorCodes.ValidationError;
                    break;

                default:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "An unexpected error occurred.";
                    errorResponse.ErrorCode = Constants.ErrorCodes.InternalError;
                    break;
            }

            response.StatusCode = errorResponse.StatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(errorResponse, options);
            return response.WriteAsync(json);
        }

        private class ErrorResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? ErrorCode { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
            public string? TraceId { get; set; }
        }
    }
}

