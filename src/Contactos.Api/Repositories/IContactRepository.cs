using Contactos.Api.Core.Domain;

namespace Contactos.Api.Repositories;

public interface IContactRepository
{
    IReadOnlyList<Contacto> GetAll();

    Contacto? GetById(int id);

    Contacto? Add(string name, string normalizedPhone);
}
