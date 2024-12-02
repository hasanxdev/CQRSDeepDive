using System.Text;
using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.Models;
using CQRSDeepDive.ReadStack.Queries;
using CQRSDeepDive.WriteStack.Events;
using KafkaFlow;
using KafkaFlow.Producers;
using MediatR;

namespace CQRSDeepDive.WriteStack.Consumers;

public class NewProductConsumer(ApplicationWriteDbContext writeDbContext, IMediator mediator, IProducerAccessor producerAccessor) : IMessageHandler<Product>
{
    public async Task Handle(IMessageContext context, Product message)
    {
        var product = await mediator.Send(new GetProductByNameQuery()
        {
            Name = message.Name
        });

        if (product is not null)
        {
            throw new Exception($"Product with name: {message.Name} already exists.");
        }

        if (message.Quantity <= 0)
        {
            throw new Exception($"Quantity is zero or negative.");
        }

        var productEntity = new Product()
        {
            Id = 0,
            Name = message.Name,
            CreatedAt = DateTime.Now,
            Description = message.Description,
            Category = message.Category,
            Price = message.Price,
            Quantity = message.Quantity,
        };
        
        writeDbContext.Products.Add(productEntity);
        await writeDbContext.SaveChangesAsync(CancellationToken.None);
        
        var producer = producerAccessor.GetProducer("Products");
        await producer.ProduceAsync(Encoding.UTF8.GetBytes(productEntity.Id.ToString()), new ProductCreated()
        {
            Id = productEntity.Id,
            Category = productEntity.Category,
            Price = productEntity.Price,
            Quantity = productEntity.Quantity,
            CreatedAt = productEntity.CreatedAt,
            Name = productEntity.Name,
            Description = productEntity.Description,
        });
    }
}