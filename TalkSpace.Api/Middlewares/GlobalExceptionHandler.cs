using System.Net;
using Domain.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TalkSpace.Api.Extensions;

namespace TalkSpace.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(IHostEnvironment env)
        {
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Create structured log with Serilog
            Log.Error(exception, "Unhandled exception occurred: {Message} | Path: {Path}",
                exception.Message,
                httpContext.Request.Path);

            var statusCode = HttpStatusCode.InternalServerError;
            var errorTitle = "Internal Server Error";
            var errorDetail = _env.IsDevelopment()
                ? exception.ToString()
                : "An unexpected error occurred. Please try again later.";

            // Enhanced exception handling
            switch (exception)
            {
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorTitle = "Unauthorized";
                    errorDetail = "Access to this resource is denied.";
                    break;

                case KeyNotFoundException or FileNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorTitle = "Resource Not Found";
                    errorDetail = "The requested resource was not found.";
                    break;

                case ArgumentException or ArgumentNullException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorTitle = "Invalid Request";
                    errorDetail = exception.Message;
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.Conflict;
                    errorTitle = "Invalid Operation";
                    errorDetail = exception.Message;
                    break;

                case NotImplementedException:
                    statusCode = HttpStatusCode.NotImplemented;
                    errorTitle = "Not Implemented";
                    errorDetail = "This feature is not available yet.";
                    break;

                case TimeoutException:
                    statusCode = HttpStatusCode.RequestTimeout;
                    errorTitle = "Request Timeout";
                    errorDetail = "The operation timed out.";
                    break;

                case DbUpdateException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorTitle = "Database Error";
                    errorDetail = "An error occurred while saving data.";
                    Log.Error(exception, "Database update exception occurred");
                    break;

                case SecurityTokenException:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorTitle = "Authentication Failed";
                    errorDetail = "Invalid security token provided.";
                    break;
            }

            // Add additional context to logs
            Log.ForContext("StatusCode", (int)statusCode)
               .ForContext("RequestMethod", httpContext.Request.Method)
               .ForContext("RequestHeaders", httpContext.Request.Headers, destructureObjects: true)
               .Error("Request failed with {StatusCode}: {ErrorTitle}", statusCode, errorTitle);

            // Return consistent error response
            httpContext.Response.StatusCode = (int)statusCode;
            await httpContext.Response.WriteAsJsonAsync(
                Result<object>.Failure(errorDetail),
                cancellationToken);

            return true;
        }
    }
}