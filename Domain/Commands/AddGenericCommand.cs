using MediatR;

namespace Domain.Commands;

/// <summary>
/// Generic command for adding/creating new entities
/// </summary>
/// <typeparam name="TEntity">The entity type to create</typeparam>
/// <typeparam name="TDto">The DTO type for response</typeparam>
public class AddGenericCommand<TEntity, TDto> : IRequest<TDto> where TEntity : class
{
    public TEntity Entity { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public AddGenericCommand(TEntity entity, string createdBy = "system")
    {
        Entity = entity;
        CreatedBy = createdBy;
    }
}

/// <summary>
/// Generic command result wrapper
/// </summary>
/// <typeparam name="TDto">DTO type</typeparam>
public class CommandResult<TDto>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TDto? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static CommandResult<TDto> SuccessResult(TDto data, string message = "Operation successful")
    {
        return new CommandResult<TDto>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static CommandResult<TDto> FailureResult(string message, List<string>? errors = null)
    {
        return new CommandResult<TDto>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string> { message }
        };
    }
}