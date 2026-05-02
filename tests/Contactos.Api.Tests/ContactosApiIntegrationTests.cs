using System.Net;
using System.Net.Http.Json;
using Contactos.Api.Core.ApiVersioning;
using Contactos.Api.Core.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Contactos.Api.Tests;

public sealed class ContactosApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ContactosApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_and_Get_by_id_return_201_and_200()
    {
        var createResponse = await _client.PostAsJsonAsync(
            ApiRouteTemplates.V1ContactsPath,
            new CreateContactoRequestDto { Nombre = "Juan Perez", Telefono = "123456789" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ContactoResponseDto>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        var getResponse = await _client.GetAsync($"{ApiRouteTemplates.V1ContactsPath}/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var roundTrip = await getResponse.Content.ReadFromJsonAsync<ContactoResponseDto>();
        Assert.Equal(created.Id, roundTrip?.Id);
        Assert.Equal("Juan Perez", roundTrip?.Nombre);
        Assert.Equal("123456789", roundTrip?.Telefono);
    }

    [Fact]
    public async Task Get_by_unknown_id_returns_404()
    {
        var response = await _client.GetAsync($"{ApiRouteTemplates.V1ContactsPath}/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_duplicate_telephone_returns_409()
    {
        var body = new CreateContactoRequestDto { Nombre = "A", Telefono = "5551234567" };
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync(ApiRouteTemplates.V1ContactsPath, body)).StatusCode);

        var dup = await _client.PostAsJsonAsync(ApiRouteTemplates.V1ContactsPath, new CreateContactoRequestDto
        {
            Nombre = "B",
            Telefono = "5551234567",
        });

        Assert.Equal(HttpStatusCode.Conflict, dup.StatusCode);
    }

    [Fact]
    public async Task Post_invalid_telephone_returns_400()
    {
        var res = await _client.PostAsJsonAsync(
            ApiRouteTemplates.V1ContactsPath,
            new CreateContactoRequestDto { Nombre = "A", Telefono = "not-a-number" });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Concurrent_posts_same_telephone_yield_one_created_and_rest_conflict()
    {
        const string sharedPhone = "91111111111";
        const int parallel = 50;
        var tasks = Enumerable.Range(0, parallel).Select(i =>
            _client.PostAsJsonAsync(
                ApiRouteTemplates.V1ContactsPath,
                new CreateContactoRequestDto { Nombre = $"ConcurrentUser{i}", Telefono = sharedPhone }));
        var responses = await Task.WhenAll(tasks);

        var createdCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, createdCount);
        Assert.Equal(parallel - 1, conflictCount);
    }
}
