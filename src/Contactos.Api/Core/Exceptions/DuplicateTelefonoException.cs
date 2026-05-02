namespace Contactos.Api.Core.Exceptions;

public sealed class DuplicateTelefonoException : Exception
{
    public DuplicateTelefonoException(string normalizedPhone)
        : base("Contact creation failed due to a uniqueness conflict.")
    {
        NormalizedPhone = normalizedPhone;
    }

    public string NormalizedPhone { get; }
}
