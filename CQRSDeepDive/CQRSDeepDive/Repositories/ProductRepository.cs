using CQRSDeepDive.Framework.Infrastructure;
using CQRSDeepDive.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace CQRSDeepDive.Repositories;

public class ProductRepository(ApplicationWriteDbContext writeDbContext)
{
    public async Task<IResult> Create(Product request)
    {
        var product = await writeDbContext.Products.SingleOrDefaultAsync(product => product.Name.Equals(request.Name));
        if (product is not null)
        {
            return Results.BadRequest("Product Exist!");
        }

        if (request.Quantity <= 0)
        {
            return Results.BadRequest("Quantity Cannot be less or equal to 0");
        }

        writeDbContext.Products.Add(request);
        await writeDbContext.SaveChangesAsync();

        return Results.Ok();
    }
}