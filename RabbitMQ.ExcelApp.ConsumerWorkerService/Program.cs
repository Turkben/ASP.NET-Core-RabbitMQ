using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.ExcelApp.ConsumerWorkerService;
using RabbitMQ.ExcelApp.ConsumerWorkerService.Models;
using RabbitMQ.ExcelApp.ConsumerWorkerService.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.AddDbContext<AdventureWorks2019Context>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SqlConnection"));
        });

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
