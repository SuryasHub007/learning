namespace Backend.Domain.Exceptions;

/// <summary>A domain rule was broken. Its message is safe to show to API clients.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}