using Contactos.Api.Core.Domain;
using Contactos.Api.Core.Exceptions;
using Contactos.Api.Repositories;

namespace Contactos.Api.Services.Queries;

public sealed class ContactQueries : IContactQueries
{
    private readonly IContactRepository _repository;

    public ContactQueries(IContactRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Contacto> GetAll() => _repository.GetAll();

    public Contacto GetById(int id)
    {
        return _repository.GetById(id) ?? throw new ContactoNotFoundException(id);
    }
}
