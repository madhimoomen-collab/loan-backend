using MediatR;

namespace Domain.Commands;

/// <summary>
/// Generic command for updating existing entities
/// </summary>
/// <typeparam name="TEntity">The entity type to update</typeparam>
/// <typeparam name="TDto">The DTO type for response</typeparam>
public class PutGenericCommand<TEntity, TDto> : IRequest<TDto> where TEntity : class
{
    public Guid Id { get; set; }
    public TEntity Entity { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    public PutGenericCommand(Guid id, TEntity entity, string updatedBy = "system")
    {
        Id = id;
        Entity = entity;
        UpdatedBy = updatedBy;
    }
}