using Contactos.Api.Core.Domain;

namespace Contactos.Api.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly object _lock = new();
    private readonly List<Contacto> _contacts = new();
    private int _nextId = 1;

    public IReadOnlyList<Contacto> GetAll()
    {
        lock (_lock)
        {
            return _contacts.ToArray();
        }
    }

    public Contacto? GetById(int id)
    {
        lock (_lock)
        {
            return _contacts.Find(c => c.Id == id);
        }
    }

    public Contacto? Add(string name, string normalizedPhone)
    {
        lock (_lock)
        {
            var exists = _contacts.Exists(c =>
                string.Equals(c.Telefono, normalizedPhone, StringComparison.Ordinal));
            if (exists)
            {
                return null;
            }

            var id = _nextId++;
            var entity = new Contacto(id, name, normalizedPhone);
            _contacts.Add(entity);
            return entity;
        }
    }
}
