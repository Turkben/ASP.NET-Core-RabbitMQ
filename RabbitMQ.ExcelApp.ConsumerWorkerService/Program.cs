using RabbitMQ.Client;
using RabbitMQ.ExcelApp.ConsumerWorkerService;
using RabbitMQ.ExcelApp.ConsumerWorkerService.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.AddSingleton(sp => {
            var connectionString = configuration.GetConnectionString("RabbitMQ");
            return new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                DispatchConsumersAsync = true
            };
        });
        services.AddSingleton<RabbitMQClientService>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
