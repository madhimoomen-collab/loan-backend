using MediatR;
using Domain.Models;
using System.Collections.Generic;

namespace Domain.Queries
{
    // It must implement IRequest<IEnumerable<T>> to match the Handler!
    public class GetListGenericQuery<T> : IRequest<IEnumerable<T>> where T : BaseEntity
    {
        // No properties needed for a "Get All" query
    }
}