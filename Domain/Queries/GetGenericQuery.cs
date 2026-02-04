using MediatR;
using Domain.Models;

namespace Domain.Queries
{
    public class GetGenericQuery<T> : IRequest<T?> where T : BaseEntity
    {
        public int Id { get; set; }

        public GetGenericQuery(int id)
        {
            Id = id;
        }
    }
}