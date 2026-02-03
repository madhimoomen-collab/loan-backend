using MediatR;

namespace Domain.Commands;

/// <summary>
/// Generic command for deleting entities by ID
/// </summary>
/// <typeparam name="TEntity">The entity type to delete</typeparam>
public class DeleteGenericCommand<TEntity> : IRequest<bool> where TEntity : class
{
    public Guid Id { get; set; }

    public DeleteGenericCommand(Guid id, string deletedBy = "system", bool hardDelete = false)
    {
        Id = id;

    }
}