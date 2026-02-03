using Domain.Commands;
using Domain.Interface;
using MediatR;

namespace Domain.Handlers;

/// <summary>
/// Generic handler for Add/Create commands
/// </summary>
/// <typeparam name="TEntity">Entity type from domain</typeparam>
/// <typeparam name="TDto">DTO type for response</typeparam>
public class AddGenericHandlers<TEntity, TDto> : IRequestHandler<AddGenericCommand<TEntity, TDto>, TDto>
    where TEntity : class
    where TDto : class
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IMapper<TEntity, TDto> _mapper;

    public AddGenericHandlers(
        IGenericRepository<TEntity> repository,
        IMapper<TEntity, TDto> mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TDto> Handle(AddGenericCommand<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        var addedEntity = await _repository.AddAsync(request.Entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return _mapper.MapToDto(addedEntity);
    }
}