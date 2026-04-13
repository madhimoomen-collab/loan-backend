using Domain.Commands;
using Domain.DTOs;
using Domain.Interface;
using Domain.Models;
using Domain.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJwtService _jwtService;
    private readonly IValidator<SignupRequestDto> _signupValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<ChangePasswordRequestDto> _changePasswordValidator;

    public AuthController(
        IMediator mediator,
        IJwtService jwtService,
        IValidator<SignupRequestDto> signupValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<ChangePasswordRequestDto> changePasswordValidator)
    {
        _mediator = mediator;
        _jwtService = jwtService;
        _signupValidator = signupValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequestDto request)
    {
        var signupValidation = await _signupValidator.ValidateAsync(request);
        if (!signupValidation.IsValid)
        {
            return BadRequest(signupValidation.Errors.Select(e => e.ErrorMessage));
        }

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
        var fullName = $"{createdUser.FirstName} {createdUser.LastName}";
        var roles = await GetUserRoleNames(createdUser.Id);
        var token = _jwtService.GenerateToken(createdUser.Id, createdUser.Email, fullName, roles);

        return Ok(new AuthResponseDto
        {
            UserId = createdUser.Id,
            Email = createdUser.Email,
            FullName = fullName,
            Roles = roles.ToList(),
            Token = token,
            Message = "Signup successful."
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var loginValidation = await _loginValidator.ValidateAsync(request);
        if (!loginValidation.IsValid)
        {
            return BadRequest(loginValidation.Errors.Select(e => e.ErrorMessage));
        }

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

        var fullName = $"{user.FirstName} {user.LastName}";
        var roles = await GetUserRoleNames(user.Id);
        var token = _jwtService.GenerateToken(user.Id, user.Email, fullName, roles);

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = fullName,
            Roles = roles.ToList(),
            Token = token,
            Message = "Login successful."
        });
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var changePasswordValidation = await _changePasswordValidator.ValidateAsync(request);
        if (!changePasswordValidation.IsValid)
        {
            return BadRequest(changePasswordValidation.Errors.Select(e => e.ErrorMessage));
        }

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

        var fullName = $"{updatedUser.FirstName} {updatedUser.LastName}";
        var roles = await GetUserRoleNames(updatedUser.Id);
        var token = _jwtService.GenerateToken(updatedUser.Id, updatedUser.Email, fullName, roles);

        return Ok(new AuthResponseDto
        {
            UserId = updatedUser.Id,
            Email = updatedUser.Email,
            FullName = fullName,
            Roles = roles.ToList(),
            Token = token,
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

    private async Task<IEnumerable<string>> GetUserRoleNames(Guid userId)
    {
        var userRoles = await _mediator.Send(new GetListGenericQuery<UserRole>(
            condition: x => x.UserId == userId,
            includes: q => q.Include(x => x.Role)));

        return userRoles
            .Where(x => x.Role != null)
            .Select(x => x.Role!.Name)
            .Distinct()
            .ToList();
    }
}
