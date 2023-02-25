using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.WatermarkApp.Controllers;
using IModel = RabbitMQ.Client.IModel;

namespace RabbitMQ.WatermarkApp.Services
{
    public class RabbitMQClientService:IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMQClientService> _logger;

        public static string ExchangeName = "WatermarkDirectExchange";
        public static string RoutingName = "routing-watermark";
        public static string QueueName = "queue-watermark";

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();

            //if(_channel.IsOpen)
            if (_channel is {IsOpen:true } ) 
            {
                return _channel;
            }
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName,type:ExchangeType.Direct,true,false);
            _channel.QueueDeclare(QueueName, true, false, false, null);

            _channel.QueueBind(exchange:ExchangeName, queue:QueueName,routingKey: RoutingName);
            _logger.LogInformation("RabbitMQ connection is established.");

            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            //_channel = default;
            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation($"RabbitMQ connection is closed...");
        }
    }
}
