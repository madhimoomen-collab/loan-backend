using AutoMapper;
using Domain.DTOs;
using Domain.Interface;
using Domain.Models;
using Domain.Queries;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#nullable disable warnings
namespace Domain.Handlers
{
    public class GetBooksByClientHandler : IRequestHandler<GetBooksByClientQuery, IEnumerable<BookDto>>
    {
        private readonly IGenericRepository<ClientBook> _clientBookRepository;
        private readonly IMapper _mapper;

        public GetBooksByClientHandler(
            IGenericRepository<ClientBook> clientBookRepository,
            IMapper mapper)
        {
            _clientBookRepository = clientBookRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookDto>> Handle(
            GetBooksByClientQuery request, 
            CancellationToken cancellationToken)
        {
            var clientBooks = await _clientBookRepository.FindAsync(
                cb => cb.ClientId == request.ClientId && !cb.IsReturned,
                cb => cb.Book);

            var books = clientBooks
                .Where(cb => cb.Book != null)
                .Select(cb => cb.Book)
                .DistinctBy(b => b.Id)
                .ToList();

            return _mapper.Map<IEnumerable<BookDto>>(books);
        }
    }
}