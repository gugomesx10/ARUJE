namespace Aruje.Application.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = Array.Empty<string>();
    }

    public ValidationException(IEnumerable<string> errors)
        : base("Validation error.")
    {
        Errors = errors.ToArray();
    }
}