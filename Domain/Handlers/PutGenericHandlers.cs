using Domain.Commands;
using Domain.Interface;
using MediatR;

namespace Domain.Handlers;

/// <summary>
/// Generic handler for Update/Put commands
/// </summary>
/// <typeparam name="TEntity">Entity type from domain</typeparam>
/// <typeparam name="TDto">DTO type for response</typeparam>
public class PutGenericHandlers<TEntity, TDto> : IRequestHandler<PutGenericCommand<TEntity, TDto>, TDto>
    where TEntity : class
    where TDto : class
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IMapper<TEntity, TDto> _mapper;

    public PutGenericHandlers(
        IGenericRepository<TEntity> repository,
        IMapper<TEntity, TDto> mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TDto> Handle(PutGenericCommand<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if entity exists
            var existingEntity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {request.Id} not found");
            }

            // Set audit fields
            SetAuditFields(request.Entity, request.UpdatedBy);

            // Ensure the ID is set correctly
            SetEntityId(request.Entity, request.Id);

            // Update entity
            await _repository.UpdateAsync(request.Entity, cancellationToken);

            // Save changes
            await _repository.SaveChangesAsync(cancellationToken);

            // Return updated entity as DTO
            return _mapper.MapToDto(request.Entity);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating entity: {ex.Message}", ex);
        }
    }

    private void SetAuditFields(TEntity entity, string updatedBy)
    {
        var entityType = entity.GetType();

        // Set UpdatedAt
        var updatedAtProperty = entityType.GetProperty("UpdatedAt");
        if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime?))
        {
            updatedAtProperty.SetValue(entity, DateTime.UtcNow);
        }

        // Set UpdatedBy
        var updatedByProperty = entityType.GetProperty("UpdatedBy");
        if (updatedByProperty != null && updatedByProperty.PropertyType == typeof(string))
        {
            updatedByProperty.SetValue(entity, updatedBy);
        }
    }

    private void SetEntityId(TEntity entity, Guid id)
    {
        var entityType = entity.GetType();
        var idProperty = entityType.GetProperty("Id");

        if (idProperty != null && idProperty.PropertyType == typeof(Guid))
        {
            idProperty.SetValue(entity, id);
        }
    }
}