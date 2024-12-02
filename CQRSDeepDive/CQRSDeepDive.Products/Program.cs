using System.Text.Json;
using CQRSDeepDive.Models;
using CQRSDeepDive.Products;
using KafkaFlow;
using KafkaFlow.Serializer;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");
builder.Services.AddSingleton(multiplexer);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddKafkaFlowHostedService(kafkaConfigBuilder =>
{
    kafkaConfigBuilder.AddCluster(clusterConfBuilder =>
    {
        clusterConfBuilder.WithBrokers(new[] { "localhost:29092", "localhost:29093", "localhost:29094" });
        clusterConfBuilder.WithName("CQRSDeepDiveWriteStack");

        clusterConfBuilder.AddConsumer(consumer =>
        {
            consumer.Topic("Products")
                .WithGroupId("CQRSDeepDive.Products1")
                .WithBufferSize(1)
                .WithWorkersCount(1)
                .AddMiddlewares(builder =>
                {
                    builder
                        .AddSingleTypeDeserializer<Product, NewtonsoftJsonDeserializer>(
                            resolver => new NewtonsoftJsonDeserializer())
                        .AddTypedHandlers(typedHandlerConfigurationBuilder =>
                        {
                            typedHandlerConfigurationBuilder.WithHandlerLifetime(InstanceLifetime.Transient)
                                .AddHandler<ProductConsumer>();
                        });
                });

        });
    });
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");

app.MapGet("/products/{id}", async (IConnectionMultiplexer multiplexer, int id, CancellationToken cancellationToken) =>
{
    var data = await multiplexer.GetDatabase().StringGetAsync("products:"+id);
    return JsonSerializer.Deserialize<Product>(data);
});

app.MapGet("/products", async (IConnectionMultiplexer multiplexer, CancellationToken cancellationToken) =>
{
    var server = multiplexer.GetServer("localhost", 6379);
    var products = new List<Product>();

    await foreach (var key in server.KeysAsync(pattern: "products:*"))
    {
        var stringProduct = await multiplexer.GetDatabase().StringGetAsync(key);
        products.Add(JsonSerializer.Deserialize<Product>(stringProduct));
    }

    return products; 
});



app.Run();