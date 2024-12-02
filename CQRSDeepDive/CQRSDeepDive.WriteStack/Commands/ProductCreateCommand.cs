using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.Models;
using CQRSDeepDive.ReadStack.Queries;
using CQRSDeepDive.WriteStack.Events;
using KafkaFlow.Producers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

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

public class ProductCreateCommandHandler(
    ApplicationWriteDbContext applicationWriteDbContext,
    IMediator mediator, IProducerAccessor producerAccessor) : IRequestHandler<ProductCreateCommand, int>
{
    public async Task<int> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        var producer = producerAccessor.GetProducer("NewProducts");
        await producer.ProduceAsync(null, new ProductCreated()
        {
            // Id = request.Id,
            Category = request.Category,
            Price = request.Price,
            Quantity = request.Quantity,
            CreatedAt = request.CreatedAt,
            Name = request.Name,
            Description = request.Description,
        });

        return 0;
    }
}