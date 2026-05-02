using Contactos.Api.Core.Domain;

namespace Contactos.Api.Services.Queries;

public interface IContactQueries
{
    IReadOnlyList<Contacto> GetAll();

    Contacto GetById(int id);
}
