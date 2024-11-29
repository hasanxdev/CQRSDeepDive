using System.Reflection;
using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.ReadStack.Queries;
using CQRSDeepDive.Repositories;
using CQRSDeepDive.WriteStack.Commands;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

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