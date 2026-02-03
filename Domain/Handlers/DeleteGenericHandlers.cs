using Domain.Commands;
using Domain.Interface;
using MediatR;

namespace Domain.Handlers;

/// <summary>
/// Generic handler for Delete commands
/// Supports both soft delete and hard delete
/// </summary>
/// <typeparam name="TEntity">Entity type from domain</typeparam>
public class DeleteGenericHandlers<TEntity> : IRequestHandler<DeleteGenericCommand<TEntity>, bool>
    where TEntity : class
{
    private readonly IGenericRepository<TEntity> _repository;

    public DeleteGenericHandlers(IGenericRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteGenericCommand<TEntity> request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the entity
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {request.Id} not found");
            }

            if (request.HardDelete)
            {
                // Hard delete - permanently remove from database
                await _repository.DeleteAsync(entity, cancellationToken);
            }
            else
            {
                // Soft delete - mark as deleted
                SetSoftDeleteFields(entity, request.DeletedBy);
                await _repository.UpdateAsync(entity, cancellationToken);
            }

            // Save changes
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting entity: {ex.Message}", ex);
        }
    }

    private void SetSoftDeleteFields(TEntity entity, string deletedBy)
    {
        var entityType = entity.GetType();

        // Set IsDeleted flag
        var isDeletedProperty = entityType.GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            isDeletedProperty.SetValue(entity, true);
        }

        // Set DeletedAt timestamp
        var deletedAtProperty = entityType.GetProperty("DeletedAt");
        if (deletedAtProperty != null && deletedAtProperty.PropertyType == typeof(DateTime?))
        {
            deletedAtProperty.SetValue(entity, DateTime.UtcNow);
        }

        // Set DeletedBy user
        var deletedByProperty = entityType.GetProperty("DeletedBy");
        if (deletedByProperty != null && deletedByProperty.PropertyType == typeof(string))
        {
            deletedByProperty.SetValue(entity, deletedBy);
        }
    }
}