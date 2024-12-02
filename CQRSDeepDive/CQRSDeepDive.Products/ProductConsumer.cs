using System.Text.Json;
using CQRSDeepDive.Models;
using KafkaFlow;
using StackExchange.Redis;

namespace CQRSDeepDive.Products;

public class ProductConsumer(IConnectionMultiplexer connectionMultiplexer) : IMessageHandler<Product>
{
    public async Task Handle(IMessageContext context, Product message)
    {
        await connectionMultiplexer.GetDatabase()
            .StringSetAsync("products:" + message.Id, JsonSerializer.Serialize(message));
    }
}