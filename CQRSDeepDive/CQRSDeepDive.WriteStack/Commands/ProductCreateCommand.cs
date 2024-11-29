using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.Models;
using CQRSDeepDive.ReadStack.Queries;
using MediatR;

namespace CQRSDeepDive.WriteStack.Commands;

public class ProductCreateCommand : IRequest<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Quantity { get; set; }
}

public class ProductCreateCommandHandler(ApplicationWriteDbContext applicationWriteDbContext, IMediator mediator) : IRequestHandler<ProductCreateCommand, int>
{
    public async Task<int> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductByNameQuery()
        {
            Name = request.Name
        }, cancellationToken);
        
        if (product is not null)
        {
            throw new Exception($"Product with name: {request.Name} already exists.");
        }

        if (request.Quantity <= 0)
        {
            throw new Exception($"Quantity is zero or negative.");
        }

        var productEntity = new Product()
        {
            Id = 0,
            Name = request.Name,
            CreatedAt = DateTime.Now,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            Quantity = request.Quantity,
        };
        applicationWriteDbContext.Products.Add(productEntity);
        
        await applicationWriteDbContext.SaveChangesAsync(CancellationToken.None);

        return productEntity.Id;
    }
}