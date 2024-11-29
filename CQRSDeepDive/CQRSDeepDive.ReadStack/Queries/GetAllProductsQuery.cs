using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CQRSDeepDive.ReadStack.Queries;

public class GetAllProductsQuery : IRequest<List<Product>>
{
}

public class GetAllProductsQueryHandler(ApplicationReadDbContext applicationReadDbContext) : IRequestHandler<GetAllProductsQuery, List<Product>> 
{
    public Task<List<Product>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        return applicationReadDbContext.Products.ToListAsync(cancellationToken);
    }
}