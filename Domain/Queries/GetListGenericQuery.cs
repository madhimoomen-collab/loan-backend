using MediatR;
using System.Linq.Expressions;

namespace Domain.Queries;

/// <summary>
/// Generic query for retrieving a list of entities with filtering, sorting, and pagination
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDto">The DTO type to return</typeparam>
public class GetListGenericQuery<TEntity, TDto> : IRequest<PagedResult<TDto>>
    where TEntity : class
    where TDto : class
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? OrderBy { get; set; }
    public bool OrderDescending { get; set; } = false;
    public bool IncludeDeleted { get; set; } = false;
    public Expression<Func<TEntity, bool>>? CustomFilter { get; set; }

    public GetListGenericQuery()
    {
    }

    public GetListGenericQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 ? pageSize : 10;
    }
}

/// <summary>
/// Paged result wrapper for list queries
/// </summary>
/// <typeparam name="T">Type of items in the list</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult()
    {
    }

    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty()
    {
        return new PagedResult<T>(new List<T>(), 0, 1, 10);
    }
}