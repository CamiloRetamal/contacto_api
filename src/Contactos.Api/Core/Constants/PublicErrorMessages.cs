namespace Contactos.Api.Core.Constants;

public static class PublicErrorMessages
{
    public const string NotFoundTitle = "Recurso no encontrado";
    public const string NotFoundDetail = "No se encontró el recurso solicitado.";

    public const string ConflictTitle = "Conflicto de datos";
    public const string ConflictDetail =
        "La operación no pudo completarse debido a un conflicto con el estado actual de los datos.";

    public const string ValidationTitle = "Solicitud inválida";
    public const string ValidationDetail =
        "La solicitud no pudo ser procesada. Verifique los datos enviados.";

    public const string ServerErrorTitle = "Error interno del servidor";
    public const string ServerErrorDetail =
        "Se produjo un error inesperado. Incluya el traceId al reportar el incidente.";
}
