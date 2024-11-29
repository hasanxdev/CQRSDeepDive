var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/products", async (IMediator mediator, CancellationToken cancellationToken) =>
    await mediator.Send(new GetAllProductsQuery(), cancellationToken));

app.Run();