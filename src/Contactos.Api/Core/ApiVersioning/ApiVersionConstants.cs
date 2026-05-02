using Asp.Versioning;

namespace Contactos.Api.Core.ApiVersioning;

public static class ApiVersionConstants
{
    public const int V1Major = 1;
    public const int V1Minor = 0;

    public const string V1Literal = "1.0";

    public static readonly ApiVersion V1 = new(V1Major, V1Minor);

    public static readonly ApiVersion Default = V1;
}
