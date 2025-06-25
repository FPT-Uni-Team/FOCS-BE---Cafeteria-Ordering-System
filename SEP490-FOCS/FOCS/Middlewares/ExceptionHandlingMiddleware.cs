using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FOCS.Common.Exceptions;

namespace FOCS.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate requestDelegate, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = requestDelegate;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            } catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, Errors.SystemError.UnhandledExceptionOccurred);

            context.Response.ContentType = "application/json";
            int statusCode = StatusCodes.Status500InternalServerError;
            string[] messages = exception.Message.Split("@");
            string message = messages[0];
            string fieldName = string.Empty;
            if(messages.Count() > 1)
            {
                fieldName = messages[1] ?? string.Empty;
            }
            //Check the error and res correct error
            if (exception is ValidationException)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
            }
            else if (exception is NotMappedAttribute)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = StatusCodes.Status401Unauthorized;
                message = "Unauthorized access.";
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Message = message,
                FieldName = fieldName
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
