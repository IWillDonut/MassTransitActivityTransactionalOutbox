using System.Reflection;
using Example;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(Environment.GetEnvironmentVariable("APP_SETTINGS_PATH") ?? "appsettings.json")
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Verbose)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
        (hostContext, services) =>
        {
            services.AddHostedService<ExampleHostedService>();

            services.AddDbContext<ExampleDbContext>(
                (c) =>
                {
                    var connectionString =
                        configuration.GetConnectionString("Database")
                        ?? throw new InvalidOperationException(
                            "Connection string 'Database' not found."
                        );

                    c.UseSqlServer(
                        connectionString,
                        m =>
                        {
                            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(ExampleDbContext)}");
                        }
                    );
                }
            );

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddEntityFrameworkOutbox<ExampleDbContext>(o =>
                {
                    o.UseSqlServer();

                    o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                });

                x.AddConfigureEndpointsCallback(
                    (context, name, c) =>
                    {
                        c.UseEntityFrameworkOutbox<ExampleDbContext>(context);
                    }
                );

                x.AddConsumer<ExampleConsumer>();
                x.AddExecuteActivity<ExampleActivity, ExampleArguments>();

                x.UsingRabbitMq(
                    (context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    }
                );
            });
        }
    )
    .UseSerilog()
    .Build();

await host.RunAsync();
