using CQRSDeepDive.Models;
using Microsoft.EntityFrameworkCore;

namespace CQRSDeepDive.Framework.Infrastructure;

public class ApplicationReadDbContext : DbContext
{
    public IQueryable<Product> Products => Set<Product>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=CQRSDeepDive.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToTable("Products");
        base.OnModelCreating(modelBuilder);
    }
}