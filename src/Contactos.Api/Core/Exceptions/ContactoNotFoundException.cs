namespace Contactos.Api.Core.Exceptions;

public sealed class ContactoNotFoundException : Exception
{
    public ContactoNotFoundException(int id)
        : base("Contact was not found.")
    {
        Id = id;
    }

    public int Id { get; }
}
