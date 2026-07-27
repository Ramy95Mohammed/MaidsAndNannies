namespace MaidsAndNannies.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"لم يتم العثور على \"{name}\" ({key}).")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Business-rule conflicts that aren't validation errors, e.g. "email already
/// registered", "same phone number used by a worker account". Mapped to 409.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
