using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.WordToPdfApp.Producer.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.WordToPdfApp.Producer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult WordToPdf()
        {
            return View();

        }
        [HttpPost]
        public IActionResult WordToPdf(WordToPdf wordToPdf)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(_configuration.GetConnectionString("RabbitMQ"));

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var exchangeName = "convert-exchange";
                    var queueName = "convert-pdf-queue";
                    var routeKey = "Convert-routeKey";

                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false);
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, null);
                    channel.QueueBind(queueName, exchangeName, routeKey);


                    WordToPdfMessage message = new WordToPdfMessage();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        wordToPdf.File.CopyTo(ms);
                        message.WordByte = ms.ToArray();
                    }

                    message.Email = wordToPdf.Email;
                    message.FileName = Path.GetFileNameWithoutExtension(wordToPdf.File.FileName);

                    var bodyString = JsonSerializer.Serialize(message);
                    var bodyByte = Encoding.UTF8.GetBytes(bodyString);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: exchangeName, routingKey: routeKey, basicProperties: properties, body: bodyByte);

                    _logger.LogInformation("RabbitMQ connection is established.");
                    ViewBag.result = "Word to Pdf Started";
                }

            }
            return RedirectToAction(nameof(WordToPdf));

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}