using System.Reflection;
using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.Models;
using CQRSDeepDive.ReadStack.Queries;
using CQRSDeepDive.Repositories;
using CQRSDeepDive.WriteStack;
using CQRSDeepDive.WriteStack.Commands;
using CQRSDeepDive.WriteStack.Consumers;
using KafkaFlow;
using KafkaFlow.Retry;
using KafkaFlow.Serializer;
using MediatR;
using Polly.Utilities;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

IConnectionMultiplexer multiplexer = ConnectionMultiplexer
    .Connect("localhost:6379,allowAdmin=true");
builder.Services.AddSingleton(multiplexer);

builder.Services.AddKafkaFlowHostedService(kafkaConfigBuilder =>
{
    kafkaConfigBuilder.AddCluster(clusterConfBuilder =>
    {
        clusterConfBuilder.WithBrokers(new[] { "localhost:29092", "localhost:29093", "localhost:29094" });
        clusterConfBuilder.WithName("CQRSDeepDiveWriteStack");

        clusterConfBuilder.AddProducer("Products", producerConfigBuilder => 
            producerConfigBuilder.DefaultTopic("Products")
                .AddMiddlewares(m => m
                    .AddSerializer<JsonCoreSerializer>()));
        
        clusterConfBuilder.AddProducer("NewProducts", producerConfigBuilder => 
            producerConfigBuilder.DefaultTopic("NewProducts")
                .AddMiddlewares(m => m
                    .AddSerializer<JsonCoreSerializer>()));
        
        clusterConfBuilder.AddConsumer(consumer =>
        {
            consumer.Topic("NewProducts")
                .WithGroupId("CQRSDeepDive.WriteStack")
                .WithBufferSize(1)
                .WithWorkersCount(1)
                .AddMiddlewares(builder =>
                {
                    builder.RetryForever(retry => retry
                            .Handle(RetryIsRequired)
                            .WithTimeBetweenTriesPlan(static (tryCount) =>
                                new[] {
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(2),
                                        TimeSpan.FromSeconds(4)
                                    }
                                    [tryCount % 3]
                            ))
                        .AddSingleTypeDeserializer<Product, NewtonsoftJsonDeserializer>(
                            resolver => new NewtonsoftJsonDeserializer())
                        .AddTypedHandlers(typedHandlerConfigurationBuilder =>
                        {
                            typedHandlerConfigurationBuilder.WithHandlerLifetime(InstanceLifetime.Transient)
                                
                                .AddHandler<NewProductConsumer>();
                        });
                });

            static bool RetryIsRequired(RetryContext context)
            {
                return 
                    context.Exception is ArgumentNullException ||
                    context.Exception.Message.Contains("Task was cancelled.");
            }
        });
    });
});

builder.Services.AddMediatR(cfg=>
        cfg.RegisterServicesFromAssemblies([Assembly.GetExecutingAssembly(),
            typeof(GetAllProductsQuery).Assembly,
            typeof(ProductCreateCommand).Assembly]));
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationWriteDbContext>();
builder.Services.AddDbContext<ApplicationReadDbContext>();
builder.Services.AddScoped<ProductRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
    var dbRead = scope.ServiceProvider.GetRequiredService<ApplicationReadDbContext>();
    db.Database.EnsureCreated();
    dbRead.Database.EnsureCreated();
}

app.MapPost("/products/add", async (IMediator mediator, ProductCreateCommand command, CancellationToken cancellationToken) => 
    await mediator.Send(command, cancellationToken));

app.MapGet("/", () => "Hello World!");

app.Run();