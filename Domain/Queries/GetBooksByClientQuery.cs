using Domain.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Domain.Queries
{
    public class GetBooksByClientQuery : IRequest<IEnumerable<BookDto>>
    {
        public int ClientId { get; }

        public GetBooksByClientQuery(int clientId)
        {
            ClientId = clientId;
        }
    }
}