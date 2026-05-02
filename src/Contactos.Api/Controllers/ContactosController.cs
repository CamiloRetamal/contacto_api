using Asp.Versioning;
using Contactos.Api.Application.Contacts;
using Contactos.Api.Core.ApiVersioning;
using Contactos.Api.Core.Constants;
using Contactos.Api.Core.Domain;
using Contactos.Api.Core.Dtos;
using Contactos.Api.Core.Middleware;
using Contactos.Api.Services.Commands;
using Contactos.Api.Services.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Contactos.Api.Controllers;

[ApiController]
[ApiVersion(ApiVersionConstants.V1Literal)]
[Route(ApiRouteTemplates.Contacts)]
public sealed class ContactosController : ControllerBase
{
    private readonly IContactQueries _queries;
    private readonly IContactCommands _commands;

    public ContactosController(IContactQueries queries, IContactCommands commands)
    {
        _queries = queries;
        _commands = commands;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContactoResponseDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ContactoResponseDto>> GetAll()
    {
        var items = _queries.GetAll().Select(Map).ToList();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContactoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ContactoResponseDto> GetById(int id)
    {
        var entity = _queries.GetById(id);
        return Ok(Map(entity));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<ContactoResponseDto> Create([FromBody] CreateContactoRequestDto request)
    {
        var version = RouteData.Values["version"]?.ToString()
                      ?? ApiVersionConstants.V1Literal;
        return _commands.Create(request) switch
        {
            ContactCreateSuccess s => CreatedAtAction(
                nameof(GetById),
                new { version, id = s.Contact.Id },
                Map(s.Contact)),
            ContactCreateDuplicate => ConflictProblem(),
            ContactCreateValidation => BadRequestProblem(),
            _ => throw new InvalidOperationException("Unhandled ContactCreateResult."),
        };
    }

    private ObjectResult ConflictProblem()
    {
        return new ObjectResult(BuildProblem(PublicErrorMessages.ConflictTitle, PublicErrorMessages.ConflictDetail, 409))
        {
            StatusCode = StatusCodes.Status409Conflict,
        };
    }

    private ObjectResult BadRequestProblem()
    {
        return new ObjectResult(BuildProblem(
                PublicErrorMessages.ValidationTitle,
                PublicErrorMessages.ValidationDetail,
                StatusCodes.Status400BadRequest))
        {
            StatusCode = StatusCodes.Status400BadRequest,
        };
    }

    private ProblemDetails BuildProblem(string title, string detail, int status)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status,
            Instance = HttpContext.Request.Path.Value ?? string.Empty,
            Extensions = { ["traceId"] = CorrelationContext.Resolve(HttpContext) },
        };
    }

    private static ContactoResponseDto Map(Contacto c) => new(c.Id, c.Nombre, c.Telefono);
}
