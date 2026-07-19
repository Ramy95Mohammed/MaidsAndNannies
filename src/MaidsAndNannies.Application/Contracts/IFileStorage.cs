namespace MaidsAndNannies.Application.Contracts;

public interface IFileStorage
{
    Task<string> SavePrivateAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
}
