using System.Diagnostics;
using MassTransit;

namespace Example;

public record ExampleArguments;

public class ExampleActivity(ILogger<ExampleActivity> logger, ExampleDbContext dbContext)
    : IExecuteActivity<ExampleArguments>
{
    private readonly ExampleDbContext _dbContext = dbContext;
    private readonly ILogger<ExampleActivity> _logger = logger;

    public async Task<ExecutionResult> Execute(ExecuteContext<ExampleArguments> context)
    {
        _logger.LogInformation(
            "ExampleActivity - Transaction({@CurrentTransaction})",
            _dbContext.Database.CurrentTransaction?.TransactionId
        );

        await context.Publish(new Hello(":)"));

        await Task.Delay(TimeSpan.FromSeconds(10), context.CancellationToken);

        return context.Completed();
    }
}
