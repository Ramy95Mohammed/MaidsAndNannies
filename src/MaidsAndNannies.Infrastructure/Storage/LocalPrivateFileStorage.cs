using MaidsAndNannies.Application.Contracts;
using Microsoft.Extensions.Hosting;

namespace MaidsAndNannies.Infrastructure.Storage;

public sealed class LocalPrivateFileStorage(IHostEnvironment environment) : IFileStorage
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public async Task<string> SavePrivateAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var storageFolder = Path.Combine(environment.ContentRootPath, "private-uploads");
        Directory.CreateDirectory(storageFolder);

        var storageKey = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(storageFolder, storageKey);
        await using var output = File.Create(filePath);
        await content.CopyToAsync(output, cancellationToken);
        return storageKey;
    }

    public async Task<string> SavePublicAsync(Stream content, string fileName, string subFolder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            throw new InvalidOperationException("صيغة الصورة غير مدعومة");

        if (content.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("حجم الصورة أكبر من الحد المسموح به (5 ميجابايت)");

        var uploadsRoot = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads", subFolder);
        Directory.CreateDirectory(uploadsRoot);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, storedFileName);

        await using (var output = File.Create(filePath))
        {
            await content.CopyToAsync(output, cancellationToken);
        }

        return $"/uploads/{subFolder}/{storedFileName}";
    }

    public void DeletePublic(string relativeUrl)
    {
        try
        {
            var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(environment.ContentRootPath, "wwwroot", relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch
        {
            // Best-effort cleanup only - a stray file should never fail the request.
        }
    }
}
