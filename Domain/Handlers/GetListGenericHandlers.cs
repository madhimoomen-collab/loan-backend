using Domain.Interface;
using Domain.Queries;
using MediatR;
using System.Linq.Expressions;

namespace Domain.Handlers;

/// <summary>
/// Generic handler for Get list queries with filtering, sorting, and pagination
/// </summary>
/// <typeparam name="TEntity">Entity type from domain</typeparam>
/// <typeparam name="TDto">DTO type for response</typeparam>
public class GetListGenericHandlers<TEntity, TDto> : IRequestHandler<GetListGenericQuery<TEntity, TDto>, PagedResult<TDto>>
    where TEntity : class
    where TDto : class
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IMapper<TEntity, TDto> _mapper;

    public GetListGenericHandlers(
        IGenericRepository<TEntity> repository,
        IMapper<TEntity, TDto> mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TDto>> Handle(GetListGenericQuery<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        try
        {
            // Build filter expression
            Expression<Func<TEntity, bool>>? filter = BuildFilter(request);

            // Build order by function
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = BuildOrderBy(request);

            // Get paged data
            var (entities, totalCount) = await _repository.GetPagedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                filter: filter,
                orderBy: orderBy,
                cancellationToken: cancellationToken
            );

            // Map entities to DTOs
            var dtos = entities.Select(e => _mapper.MapToDto(e)).ToList();

            return new PagedResult<TDto>(
                items: dtos,
                totalCount: totalCount,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving entity list: {ex.Message}", ex);
        }
    }

    private Expression<Func<TEntity, bool>>? BuildFilter(GetListGenericQuery<TEntity, TDto> request)
    {
        Expression<Func<TEntity, bool>>? filter = null;

        // Start with custom filter if provided
        if (request.CustomFilter != null)
        {
            filter = request.CustomFilter;
        }

        // Add soft delete filter
        if (!request.IncludeDeleted)
        {
            Expression<Func<TEntity, bool>> deletedFilter = e =>
                EF.Property<bool>(e, "IsDeleted") == false;

            filter = filter == null ? deletedFilter : CombineFilters(filter, deletedFilter);
        }

        return filter;
    }

    private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? BuildOrderBy(GetListGenericQuery<TEntity, TDto> request)
    {
        if (string.IsNullOrWhiteSpace(request.OrderBy))
        {
            // Default ordering by CreatedAt descending
            return query => query.OrderByDescending(e => EF.Property<DateTime>(e, "CreatedAt"));
        }

        // Custom ordering based on OrderBy property name
        return query =>
        {
            if (request.OrderDescending)
            {
                return query.OrderByDescending(e => EF.Property<object>(e, request.OrderBy));
            }
            else
            {
                return query.OrderBy(e => EF.Property<object>(e, request.OrderBy));
            }
        };
    }

    private Expression<Func<TEntity, bool>> CombineFilters(
        Expression<Func<TEntity, bool>> first,
        Expression<Func<TEntity, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(TEntity));

        var combined = Expression.AndAlso(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter)
        );

        return Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
    }
}

// Helper class for EF.Property
public static class EF
{
    public static TProperty Property<TProperty>(object entity, string propertyName)
    {
        var property = entity.GetType().GetProperty(propertyName);
        if (property == null)
        {
            return default!;
        }
        return (TProperty)property.GetValue(entity)!;
    }
}