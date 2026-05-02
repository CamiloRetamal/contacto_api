using Contactos.Api.Application.Contacts;
using Contactos.Api.Core.Dtos;
using Contactos.Api.Repositories;
using Contactos.Api.Services.Commands;
using Microsoft.Extensions.Logging.Abstractions;

namespace Contactos.Api.Tests;

public sealed class ContactCommandsTests
{
    [Fact]
    public void Create_returns_duplicate_when_telephone_already_exists()
    {
        var repo = new ContactRepository();
        var commands = new ContactCommands(repo, NullLogger<ContactCommands>.Instance);
        commands.Create(new CreateContactoRequestDto { Nombre = "Ana", Telefono = "56911112222" });

        var second = commands.Create(new CreateContactoRequestDto { Nombre = "Bob", Telefono = "56911112222" });
        var dup = Assert.IsType<ContactCreateDuplicate>(second);
        Assert.Equal("56911112222", dup.NormalizedPhone);
    }

    [Fact]
    public void Create_returns_validation_for_dangerous_or_non_numeric_telephone()
    {
        var commands = new ContactCommands(new ContactRepository(), NullLogger<ContactCommands>.Instance);
        var result = commands.Create(new CreateContactoRequestDto
        {
            Nombre = "A",
            Telefono = "'; DROP TABLE contactos;--",
        });
        Assert.IsType<ContactCreateValidation>(result);
    }

    [Fact]
    public void Create_same_number_different_format_returns_duplicate()
    {
        var repo = new ContactRepository();
        var commands = new ContactCommands(repo, NullLogger<ContactCommands>.Instance);
        commands.Create(new CreateContactoRequestDto { Nombre = "A", Telefono = "+56 9 1111-2222" });

        var second = commands.Create(new CreateContactoRequestDto { Nombre = "B", Telefono = "56911112222" });
        Assert.IsType<ContactCreateDuplicate>(second);
    }
}
