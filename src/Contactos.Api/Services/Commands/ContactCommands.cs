using Contactos.Api.Application.Contacts;
using Contactos.Api.Core.Constants;
using Contactos.Api.Core.Dtos;
using Contactos.Api.Core.Exceptions;
using Contactos.Api.Core.Validation;
using Contactos.Api.Repositories;
using Microsoft.Extensions.Logging;

namespace Contactos.Api.Services.Commands;

public sealed class ContactCommands : IContactCommands
{
    private readonly IContactRepository _repository;
    private readonly ILogger<ContactCommands> _logger;

    public ContactCommands(IContactRepository repository, ILogger<ContactCommands> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public ContactCreateResult Create(CreateContactoRequestDto request)
    {
        var name = (request.Nombre ?? string.Empty).Trim();
        var phoneRaw = (request.Telefono ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phoneRaw))
        {
            _logger.LogInformation(
                "Contact create rejected: missing name or telephone (Nombre empty: {NombreEmpty}, Telefono empty: {TelefonoEmpty}).",
                string.IsNullOrEmpty(name),
                string.IsNullOrEmpty(phoneRaw));
            return new ContactCreateValidation(PublicErrorMessages.ValidationDetailMissingFields);
        }

        string phoneDigits;
        try
        {
            phoneDigits = TelephoneParser.ParseAndValidate(phoneRaw);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogInformation(ex, "Contact create rejected: invalid telephone for {Nombre}.", name);
            return new ContactCreateValidation(PublicErrorMessages.ValidationDetailInvalidTelephone);
        }

        var created = _repository.Add(name, phoneDigits);
        if (created is null)
        {
            _logger.LogWarning(
                "Contact create rejected: duplicate telephone {Telefono} (requested name {Nombre}).",
                phoneDigits,
                name);
            return new ContactCreateDuplicate(phoneDigits);
        }

        _logger.LogInformation(
            "Contact created: Id={ContactId}, Nombre={Nombre}, Telefono={Telefono}.",
            created.Id,
            created.Nombre,
            created.Telefono);
        return new ContactCreateSuccess(created);
    }
}
