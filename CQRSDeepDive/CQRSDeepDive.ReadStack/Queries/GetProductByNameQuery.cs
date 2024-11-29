using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CQRSDeepDive.ReadStack.Queries;

public class GetProductByNameQuery : IRequest<Product?>
{
    public required string Name { get; set; }
}

public class GetProductByNameQueryHandler(ApplicationReadDbContext applicationReadDbContext) : IRequestHandler<GetProductByNameQuery, Product?>
{
    public Task<Product?> Handle(GetProductByNameQuery request, CancellationToken cancellationToken)
    {
        return applicationReadDbContext.Products
            .SingleOrDefaultAsync(product => product.Name.Equals(request.Name), cancellationToken: cancellationToken);
    }
}

