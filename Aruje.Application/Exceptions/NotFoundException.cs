namespace Aruje.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string resource, object identifier)
        : base($"{resource} not found. Identifier: {identifier}")
    {
    }
}