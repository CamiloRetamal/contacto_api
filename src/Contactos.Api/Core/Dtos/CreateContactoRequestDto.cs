using System.ComponentModel.DataAnnotations;

namespace Contactos.Api.Core.Dtos;

public sealed class CreateContactoRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string Telefono { get; init; } = string.Empty;
}
