using Domain.Commands;
using Domain.DTOs;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequestDto request)
    {
        var existingUsers = await _mediator.Send(new GetListGenericQuery<User>(
            condition: x => x.Email == request.Email));

        if (existingUsers.Any())
        {
            return BadRequest("Email is already registered.");
        }

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = HashPassword(request.Password, saltBytes);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordSalt = Convert.ToBase64String(saltBytes),
            PasswordHash = Convert.ToBase64String(hashBytes),
            IsActive = true
        };

        var createdUser = await _mediator.Send(new AddGenericCommand<User>(user));

        return Ok(new AuthResponseDto
        {
            UserId = createdUser.Id,
            Email = createdUser.Email,
            FullName = $"{createdUser.FirstName} {createdUser.LastName}",
            Message = "Signup successful."
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var users = await _mediator.Send(new GetListGenericQuery<User>(
            condition: x => x.Email == request.Email && x.IsActive));

        var user = users.FirstOrDefault();
        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        if (!VerifyPassword(request.Password, user.PasswordSalt, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Message = "Login successful."
        });
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var user = await _mediator.Send(new GetGenericQuery<User>(request.UserId));
        if (user == null || !user.IsActive)
        {
            return NotFound("User not found.");
        }

        if (!VerifyPassword(request.CurrentPassword, user.PasswordSalt, user.PasswordHash))
        {
            return Unauthorized("Current password is incorrect.");
        }

        var newSalt = RandomNumberGenerator.GetBytes(16);
        var newHash = HashPassword(request.NewPassword, newSalt);

        user.PasswordSalt = Convert.ToBase64String(newSalt);
        user.PasswordHash = Convert.ToBase64String(newHash);

        var updatedUser = await _mediator.Send(new UpdateGenericCommand<User>(user));

        return Ok(new AuthResponseDto
        {
            UserId = updatedUser.Id,
            Email = updatedUser.Email,
            FullName = $"{updatedUser.FirstName} {updatedUser.LastName}",
            Message = "Password changed successfully."
        });
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);
    }

    private static bool VerifyPassword(string password, string storedSalt, string storedHash)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        var computedHash = HashPassword(password, saltBytes);
        var storedHashBytes = Convert.FromBase64String(storedHash);

        return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHash);
    }
}
