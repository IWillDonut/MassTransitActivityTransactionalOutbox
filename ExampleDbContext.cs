using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Example;

public class ExampleDbContext(DbContextOptions<ExampleDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddTransactionalOutboxEntities();
    }
}
