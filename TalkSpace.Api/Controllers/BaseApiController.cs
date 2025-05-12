using Domain.Results;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace TalkSpace.Api.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.IsEmpty
                    ? NoContent() 
                    : Ok(new ApiResponse<T>(result.Value, result.Message));
            }

            LogError(result.Message, result.ErrorSource);
            return HandleError(result.Message, result.ErrorSource);
        }

        protected IActionResult HandleQueryResult<T>(QueryResult<T> result)
        {
            if (result.IsNotFound)
                return NotFound(new ApiResponse<object>(null, "Resource not found."));

            if (result.IsSuccess)
                return Ok(new ApiResponse<T>(result.Value, result.Message));

            LogError(result.Message, result.ErrorSource);
            return HandleError(result.Message, result.ErrorSource);
        }

        private void LogError(string? error, ErrorSource errorSource)
        {
            Log.Error(
                "API Error | Source: {ErrorSource} | Path: {Path} | Error: {Error}",
                errorSource,
                HttpContext.Request.Path,
                error
            );
        }

        private IActionResult HandleError(string? error, ErrorSource errorSource)
        {
            return errorSource switch
            {
                ErrorSource.Database => Problem(
                    title: "Database Error",
                    detail: error,
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                ),
                ErrorSource.TalkSpaceAPI => Problem(
                    title: "Invalid Request",
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    instance: HttpContext.Request.Path
                ),
                _ => Problem(
                    title: "Internal Server Error",
                    detail: "An unexpected error occurred.",
                    statusCode: StatusCodes.Status500InternalServerError
                )
            };
        }

        public record ApiResponse<T>(T? Data, string? Message = null);
    }
}