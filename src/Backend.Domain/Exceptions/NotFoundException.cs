namespace Backend.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.") { }
}