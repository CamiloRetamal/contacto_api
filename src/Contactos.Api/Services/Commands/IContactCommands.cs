using Contactos.Api.Application.Contacts;
using Contactos.Api.Core.Dtos;

namespace Contactos.Api.Services.Commands;

public interface IContactCommands
{
    ContactCreateResult Create(CreateContactoRequestDto request);
}
