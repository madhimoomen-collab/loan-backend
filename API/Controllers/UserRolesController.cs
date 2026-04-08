using Domain.Commands;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserRolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserRolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<UserRole>> Create([FromBody] UserRole model)
    {
        var user = await _mediator.Send(new GetGenericQuery<User>(model.UserId));
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var role = await _mediator.Send(new GetGenericQuery<Role>(model.RoleId));
        if (role == null)
        {
            return BadRequest("Role not found.");
        }

        var existing = await _mediator.Send(new GetListGenericQuery<UserRole>(
            condition: x => x.UserId == model.UserId && x.RoleId == model.RoleId));

        if (existing.Any())
        {
            return BadRequest("This user-role link already exists.");
        }

        var result = await _mediator.Send(new AddGenericCommand<UserRole>(model));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteGenericCommand<UserRole>(id));
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
