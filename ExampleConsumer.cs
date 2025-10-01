using System.Data.Common;
using MassTransit;
using Serilog;

namespace Example;

public record Hello(string Message);

public class ExampleConsumer(ILogger<ExampleConsumer> logger, ExampleDbContext dbContext)
    : IConsumer<Hello>
{
    private readonly ILogger<ExampleConsumer> _logger = logger;
    private readonly ExampleDbContext _dbContext = dbContext;

    public Task Consume(ConsumeContext<Hello> context)
    {
        _logger.LogInformation(
            "ExampleConsumer - Transaction({@CurrentTransaction})",
            _dbContext.Database.CurrentTransaction?.TransactionId
        );

        return Task.CompletedTask;
    }
}
