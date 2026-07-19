using MaidsAndNannies.Application.Contracts;
using Microsoft.Extensions.Hosting;

namespace MaidsAndNannies.Infrastructure.Storage;

public sealed class LocalPrivateFileStorage(IHostEnvironment environment) : IFileStorage
{
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
}
