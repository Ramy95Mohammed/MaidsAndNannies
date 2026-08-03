using System.Text.Json;
using System.Text.RegularExpressions;

namespace MaidsAndNannies.WebApi.Localization;

/// <summary>
/// Loads the ar/en message catalogs (wwwroot/localization/messages.*.json)
/// once at startup and translates API messages on demand.
/// </summary>
public static class MessageLocalizer
{
    private static readonly Dictionary<string, string> Ar = new();
    private static readonly Dictionary<string, string> En = new();
    private static bool _initialized;

    public static void Initialize(string webRootPath)
    {
        if (_initialized) return;
        _initialized = true;

        Load(webRootPath, "messages.ar.json", Ar);
        Load(webRootPath, "messages.en.json", En);
    }

    private static void Load(string webRootPath, string fileName, Dictionary<string, string> target)
    {
        try
        {
            var path = Path.Combine(webRootPath, "localization", fileName);
            if (!File.Exists(path)) return;
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path, System.Text.Encoding.UTF8));
            if (dict is null) return;
            foreach (var (k, v) in dict) target[k] = v;
        }
        catch
        {
            // catalog is optional; untranslated messages fall back to the source text
        }
    }

    public static string Translate(string text, string lang)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var en = lang.Equals("en", StringComparison.OrdinalIgnoreCase);
        var target = en ? En : Ar;

        if (target.TryGetValue(text, out var direct)) return direct;

        // Joined lists (Identity descriptions joined with "، " or ", ")
        foreach (var separator in new[] { "، ", ", " })
        {
            if (!text.Contains(separator)) continue;
            var parts = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1 && parts.All(p => target.ContainsKey(p)))
                return string.Join(en ? ", " : "، ", parts.Select(p => target[p]));
        }

        // Dynamic templates
        var notFound = Regex.Match(text, "^لم يتم العثور على \"(.+)\" \\((\\d+)\\)\\.$");
        if (notFound.Success)
            return en
                ? $"\"{notFound.Groups[1].Value}\" ({notFound.Groups[2].Value}) was not found."
                : text;

        if (!en)
        {
            if (Regex.IsMatch(text, "^Email '(.+)' is already taken\\.$"))
                return Regex.Replace(text, "^Email '(.+)' is already taken\\.$", "البريد الإلكتروني '$1' مسجل بالفعل.");
            if (Regex.IsMatch(text, "^Username '(.+)' is already taken\\.$"))
                return Regex.Replace(text, "^Username '(.+)' is already taken\\.$", "اسم المستخدم '$1' مسجل بالفعل.");
            if (Regex.IsMatch(text, "^Email '(.+)' is invalid\\.$"))
                return Regex.Replace(text, "^Email '(.+)' is invalid\\.$", "البريد الإلكتروني '$1' غير صالح.");
            if (Regex.IsMatch(text, "^Passwords must be at least (\\d+) characters\\.$"))
                return Regex.Replace(text, "^Passwords must be at least (\\d+) characters\\.$", "يجب أن تكون كلمة المرور $1 أحرف على الأقل.");
            if (Regex.IsMatch(text, "^The (.+) field is required\\.$"))
                return Regex.Replace(text, "^The (.+) field is required\\.$", "حقل '$1' مطلوب.");
            if (Regex.IsMatch(text, "^The (.+) field is not valid\\.$"))
                return Regex.Replace(text, "^The (.+) field is not valid\\.$", "قيمة حقل '$1' غير صحيحة.");
        }

        return text;
    }
}