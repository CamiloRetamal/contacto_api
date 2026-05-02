using Contactos.Api.Core.Domain;

namespace Contactos.Api.Application.Contacts;

public abstract record ContactCreateResult;

public sealed record ContactCreateSuccess(Contacto Contact) : ContactCreateResult;

public sealed record ContactCreateDuplicate(string NormalizedPhone) : ContactCreateResult;

public sealed record ContactCreateValidation(string Message) : ContactCreateResult;
