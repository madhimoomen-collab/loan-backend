using Domain.Interface;
using Domain.Queries;
using MediatR;

namespace Domain.Handlers;

/// <summary>
/// Generic handler for Get single entity queries
/// </summary>
/// <typeparam name="TEntity">Entity type from domain</typeparam>
/// <typeparam name="TDto">DTO type for response</typeparam>
public class GetGenericHandlers<TEntity, TDto> : IRequestHandler<GetGenericQuery<TDto>, TDto>
    where TEntity : class
    where TDto : class
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IMapper<TEntity, TDto> _mapper;

    public GetGenericHandlers(
        IGenericRepository<TEntity> repository,
        IMapper<TEntity, TDto> mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TDto> Handle(GetGenericQuery<TDto> request, CancellationToken cancellationToken)
    {
        try
        {
            // Get entity by ID
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {request.Id} not found");
            }

            // Check if entity is soft deleted
            if (!request.IncludeDeleted && IsDeleted(entity))
            {
                throw new KeyNotFoundException($"Entity with ID {request.Id} not found or has been deleted");
            }

            // Map to DTO and return
            return _mapper.MapToDto(entity);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving entity: {ex.Message}", ex);
        }
    }

    private bool IsDeleted(TEntity entity)
    {
        var entityType = entity.GetType();
        var isDeletedProperty = entityType.GetProperty("IsDeleted");

        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var value = isDeletedProperty.GetValue(entity);
            return value != null && (bool)value;
        }

        return false;
    }
}