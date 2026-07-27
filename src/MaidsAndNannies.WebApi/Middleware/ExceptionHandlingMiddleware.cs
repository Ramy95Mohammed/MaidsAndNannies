using MaidsAndNannies.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;
using ValidationException = MaidsAndNannies.Application.Common.Exceptions.ValidationException;

namespace MaidsAndNannies.WebApi.Middleware;

/// <summary>
/// Every Command/Query handler throws domain-meaningful exceptions instead of
/// returning ActionResult directly. This middleware is the single place that
/// maps them to HTTP status codes, so controllers stay thin (just `Send` +
/// `Ok(result)`).
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteAsync(context, HttpStatusCode.BadRequest, new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(context, HttpStatusCode.NotFound, new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            await WriteAsync(context, HttpStatusCode.Conflict, new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteAsync(context, HttpStatusCode.Unauthorized, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteAsync(context, HttpStatusCode.InternalServerError,
                new { message = "حدث خطأ غير متوقع. حاول مرة أخرى لاحقاً." });
        }
    }

    private static Task WriteAsync(HttpContext context, HttpStatusCode statusCode, object payload)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
