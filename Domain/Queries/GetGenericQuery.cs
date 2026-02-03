using MediatR;

namespace Domain.Queries;

/// <summary>
/// Generic query for retrieving a single entity by ID
/// </summary>
/// <typeparam name="TDto">The DTO type to return</typeparam>
public class GetGenericQuery<TDto> : IRequest<TDto> where TDto : class
{
    public Guid Id { get; set; }

    public GetGenericQuery(Guid id)
    {
        Id = id;
    }
}