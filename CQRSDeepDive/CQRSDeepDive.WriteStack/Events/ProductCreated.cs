using System.Text.Json;
using MediatR;
using StackExchange.Redis;

namespace CQRSDeepDive.WriteStack.Events;

public class ProductCreated : INotification
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Quantity { get; set; }
}

public class ProductCreatedHandler(IConnectionMultiplexer connectionMultiplexer) : INotificationHandler<ProductCreated>
{
    public async Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);
        await connectionMultiplexer.GetDatabase()
            .StringSetAsync("products:" + notification.Id, JsonSerializer.Serialize(notification));
    }
}