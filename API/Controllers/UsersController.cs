using Domain.Commands;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] User model)
    {
        var existing = await _mediator.Send(new GetListGenericQuery<User>(
            condition: x => x.Email == model.Email));

        if (existing.Any())
        {
            return BadRequest("Email already exists.");
        }

        var result = await _mediator.Send(new AddGenericCommand<User>(model));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteGenericCommand<User>(id));
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
