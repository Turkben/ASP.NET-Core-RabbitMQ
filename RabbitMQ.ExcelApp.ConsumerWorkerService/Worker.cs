using ClosedXML;
using ClosedXML.Excel;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.ExcelApp.ConsumerWorkerService.Models;
using RabbitMQ.ExcelApp.ConsumerWorkerService.Services;
using RabbitMQ.Shared.ExcelApp;
using System.Data;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.ExcelApp.ConsumerWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly IServiceProvider _serviceProvider;

        private IModel _channel;


        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            
            _channel.BasicConsume(RabbitMQClientService.QueueName,false, consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var memoryStream = new MemoryStream();

            var wb = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(GetTable("Products"));
            wb.Worksheets.Add(ds);
            wb.SaveAs(memoryStream);

            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

            multipartFormDataContent.Add(new ByteArrayContent(memoryStream.ToArray()),"file", Guid.NewGuid().ToString() + ".xlsx");

            var baseUrl = "https://localhost:44363/api/files";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", multipartFormDataContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File ID: {createExcelMessage.FileId} was created by successfuly ");
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }
            
        }

        private DataTable GetTable(string tableName)
        {
            List<Product> products = new List<Product>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();

                products = context.Products.ToList();
            }

            DataTable table = new DataTable() { TableName= tableName };
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            products.ForEach(x =>
            {
                table.Rows.Add(x.ProductId,x.Name,x.ProductNumber,x.Color);
            });

            return table;
        }
    }
}