
using Microsoft.Build.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.WatermarkApp.Services;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.WatermarkApp.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
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
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;

        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            Task.Delay(10000).Wait();
            try
            {
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", productImageCreatedEvent.ImageName);

                var watermarkName = "Watermark Text";
                using var img = Image.FromFile(path);
                using var graphic = Graphics.FromImage(img);
                var font = new Font(FontFamily.GenericMonospace, 30, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString(watermarkName, font);
                var color = Color.Red;
                var brush = new SolidBrush(color);
                var position = new Point(img.Width / 2, img.Height / 2);

                graphic.DrawString(watermarkName, font, brush, position);
                img.Save("wwwroot/images/watermarks/" + productImageCreatedEvent.ImageName);


                img.Dispose();
                graphic.Dispose();

                _channel.BasicAck(@event.DeliveryTag, false);

            }
            catch (Exception)
            {

                throw;
            }

            return Task.CompletedTask;
            
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
