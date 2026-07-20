namespace MaidsAndNannies.Application.Contracts;

public interface IFileStorage
{
    Task<string> SavePrivateAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a file under wwwroot/uploads/{subFolder}/ and returns the relative
    /// URL ("/uploads/{subFolder}/{guid}{ext}") the SPA can load directly through
    /// static file middleware. Used for worker/homeowner document photos.
    /// </summary>
    Task<string> SavePublicAsync(Stream content, string fileName, string subFolder, CancellationToken cancellationToken = default);

    /// <summary>Best-effort delete of a previously-saved public file. Never throws.</summary>
    void DeletePublic(string relativeUrl);
}
