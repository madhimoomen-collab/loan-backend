using Domain.Commands;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Role>> Create([FromBody] Role model)
    {
        var existing = await _mediator.Send(new GetListGenericQuery<Role>(
            condition: x => x.Name == model.Name));

        if (existing.Any())
        {
            return BadRequest("Role name already exists.");
        }

        var result = await _mediator.Send(new AddGenericCommand<Role>(model));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteGenericCommand<Role>(id));
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
