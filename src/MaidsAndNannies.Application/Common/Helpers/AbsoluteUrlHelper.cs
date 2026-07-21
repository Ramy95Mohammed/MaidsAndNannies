using Microsoft.AspNetCore.Http;

namespace MaidsAndNannies.Application.Common.Helpers;

/// <summary>
/// Shared by any handler that needs to turn a stored relative file URL
/// ("/uploads/...") into an absolute one for the SPA. Kept in one place so it
/// isn't copy-pasted into every handler that returns document/photo URLs.
/// </summary>
public static class AbsoluteUrlHelper
{
    public static string? ToAbsoluteUrl(string? relativeUrl, IHttpContextAccessor httpContextAccessor)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return relativeUrl;
        if (relativeUrl.StartsWith("http://") || relativeUrl.StartsWith("https://")) return relativeUrl;

        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null) return relativeUrl;

        return $"{request.Scheme}://{request.Host}{relativeUrl}";
    }
}
