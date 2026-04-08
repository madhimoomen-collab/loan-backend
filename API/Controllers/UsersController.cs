using Domain.Commands;
using Domain.DTOs;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] CreateUserDto model)
    {
        var existing = await _mediator.Send(new GetListGenericQuery<User>(
            condition: x => x.Email == model.Email));

        if (existing.Any())
        {
            return BadRequest("Email already exists.");
        }

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            model.Password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            PasswordSalt = Convert.ToBase64String(saltBytes),
            PasswordHash = Convert.ToBase64String(hashBytes),
            IsActive = true
        };

        var result = await _mediator.Send(new AddGenericCommand<User>(user));
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
