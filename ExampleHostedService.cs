using MassTransit;

namespace Example;

public class ExampleHostedService(IBus bus) : BackgroundService
{
    private readonly IBus _bus = bus;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddActivity("Test", new Uri("queue:example_execute"));

            var routingSlip = builder.Build();

            await _bus.Execute(routingSlip);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
