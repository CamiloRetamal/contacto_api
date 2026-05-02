using Contactos.Api.Core.Exceptions;

namespace Contactos.Api.Core.Validation;

public static class TelephoneParser
{
    public const int MinDigits = 7;
    public const int MaxDigits = 15;
    public const int MaxRawLength = 40;

    public static string ParseAndValidate(string raw)
    {
        if (raw.Length > MaxRawLength)
        {
            throw new DomainValidationException("Telephone input exceeds maximum length.");
        }

        var plusSeen = false;
        for (var i = 0; i < raw.Length; i++)
        {
            var c = raw[i];
            if (IsAsciiDigit(c))
            {
                continue;
            }

            if (c is ' ' or '-' or '(' or ')')
            {
                continue;
            }

            if (c == '+')
            {
                if (i != 0 || plusSeen)
                {
                    throw new DomainValidationException("Telephone format is invalid.");
                }

                plusSeen = true;
                continue;
            }

            throw new DomainValidationException("Telephone contains invalid characters.");
        }

        var digits = ExtractAsciiDigits(raw);
        if (digits.Length < MinDigits || digits.Length > MaxDigits)
        {
            throw new DomainValidationException("Telephone length is invalid.");
        }

        return digits;
    }

    private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

    private static string ExtractAsciiDigits(string raw) =>
        string.Concat(raw.Where(IsAsciiDigit));
}
