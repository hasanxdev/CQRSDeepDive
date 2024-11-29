using CQRSDeepDive.Models;
using Microsoft.EntityFrameworkCore;

namespace CQRSDeepDive.Framework.Infrastructure;

public class ApplicationWriteDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=CQRSDeepDive.db");
}