using Contactos.Api.Core.Exceptions;
using Contactos.Api.Core.Validation;

namespace Contactos.Api.Tests;

public sealed class TelephoneParserTests
{
    [Theory]
    [InlineData("+56 9 1111-2222", "56911112222")]
    [InlineData("(56)911112222", "56911112222")]
    [InlineData("1234567890", "1234567890")]
    public void ParseAndValidate_accepts_common_formats_and_normalizes_to_digits(string raw, string expected)
    {
        Assert.Equal(expected, TelephoneParser.ParseAndValidate(raw));
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("569-1111-xxxx")]
    [InlineData("<script>56911112222</script>")]
    [InlineData("0000000000000000")]
    public void ParseAndValidate_throws_when_telephone_not_plausible(string raw)
    {
        Assert.Throws<DomainValidationException>(() => TelephoneParser.ParseAndValidate(raw));
    }
}
