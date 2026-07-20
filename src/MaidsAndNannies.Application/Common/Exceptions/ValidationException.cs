using FluentValidation.Results;

namespace MaidsAndNannies.Application.Common.Exceptions;

/// <summary>
/// Thrown by ValidationBehavior when FluentValidation rules fail before a
/// handler ever runs. Caught by the WebApi exception middleware and turned
/// into a 400 with a { field: [errors] } payload.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException() : base("تم رفض طلب واحد أو أكثر من قواعد التحقق.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}
