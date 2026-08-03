using MaidsAndNannies.WebApi.Localization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MaidsAndNannies.WebApi.Middleware;

/// <summary>
/// Buffers every JSON response and translates user-visible message fields
/// ("message" / "Message" / "errors") according to the request's
/// "currentLanguage" header (sent by the Angular languageInterceptor).
/// Runs BEFORE ExceptionHandlingMiddleware so error responses are translated too.
/// </summary>
public sealed class ResponseLocalizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        var isJson = context.Response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
        var lang = context.Request.Headers["currentLanguage"].ToString().ToLowerInvariant() == "en" ? "en" : "ar";

        buffer.Position = 0;
        string body;
        using (var reader = new StreamReader(buffer, Encoding.UTF8, leaveOpen: true))
            body = await reader.ReadToEndAsync();

        if (isJson && body.Length > 2)
        {
            var localized = LocalizeJson(body, lang);
            if (localized != body)
            {
                context.Response.ContentLength = null;
                await context.Response.WriteAsync(localized);
                return;
            }
        }

        try
        {
            System.IO.File.AppendAllText(@"C:\Users\YN\AppData\Local\Temp\opencode\mw-debug.log",
                $"[{DateTime.UtcNow:HH:mm:ss}] {context.Request.Method} {context.Request.Path} ct={context.Response.ContentType} status={context.Response.StatusCode} bufferedLen={body.Length} isJson={isJson}\n");
        }
        catch { }

        buffer.Position = 0;
        await buffer.CopyToAsync(originalBody);
    }

    private static string LocalizeJson(string json, string lang)
    {
        try
        {
            if (JsonNode.Parse(json) is not JsonObject root) return json;
            Localize(root, lang);
            return root.ToJsonString();
        }
        catch (JsonException)
        {
            return json;
        }
    }

    private static void Localize(JsonObject obj, string lang)
    {
        foreach (var prop in obj.ToList())
        {
            if (string.Equals(prop.Key, "message", StringComparison.OrdinalIgnoreCase)
                || string.Equals(prop.Key, "title", StringComparison.OrdinalIgnoreCase))
            {
                if (prop.Value is JsonValue v && v.TryGetValue<string>(out var s))
                    obj[prop.Key] = MessageLocalizer.Translate(s, lang);
            }
            else if (string.Equals(prop.Key, "errors", StringComparison.OrdinalIgnoreCase))
            {
                TranslateStrings(prop.Value, lang);
            }
        }
    }

    private static void TranslateStrings(JsonNode? node, string lang)
    {
        if (node is JsonObject o)
        {
            foreach (var p in o.ToList()) TranslateStrings(p.Value, lang);
        }
        else if (node is JsonArray a)
        {
            for (var i = 0; i < a.Count; i++)
            {
                if (a[i] is JsonValue v && v.TryGetValue<string>(out var s))
                    a[i] = MessageLocalizer.Translate(s, lang);
                else
                    TranslateStrings(a[i], lang);
            }
        }
    }
}