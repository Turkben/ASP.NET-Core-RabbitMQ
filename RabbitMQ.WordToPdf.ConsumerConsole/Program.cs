
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spire.Doc;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;
using RabbitMQ.WordToPdf.ConsumerConsole;

var factory = new ConnectionFactory();
var rabbitmQUrl = "amqp://localhost:5672"; 
factory.Uri = new Uri(rabbitmQUrl);

bool IsEmailSent=false;

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        var exchangeName = "convert-exchange";
        var queueName = "convert-pdf-queue";
        var routeKey = "Convert-routeKey";

        //channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false);
        //channel.QueueBind(queueName, exchangeName, null);

        channel.BasicQos(0, 1, false);
        var consumer = new EventingBasicConsumer(channel);
        channel.BasicConsume(queueName, false, consumer);

        consumer.Received += (sender, args) =>
        {
            try
            {
                Console.WriteLine("Message is taken form queue...");

                Document document = new Document();

                var wordToPdfMessage = JsonSerializer.Deserialize<WordToPdfMessage>(Encoding.UTF8.GetString(args.Body.ToArray()));

                MemoryStream wordToPdfMemoryStream = new MemoryStream(wordToPdfMessage.WordByte);

                document.LoadFromStream(wordToPdfMemoryStream, FileFormat.Docx2013);

                using (MemoryStream ms = new MemoryStream())
                {
                    document.SaveToStream(ms, FileFormat.PDF);

                    IsEmailSent = EmailService.EmailSend(wordToPdfMessage.Email, ms, wordToPdfMessage.FileName);                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            if (IsEmailSent)
            {
                channel.BasicAck(args.DeliveryTag, false);
                Console.WriteLine("Message was sent succesfully");

            }
        };

        Console.WriteLine("Enter to Exit!");
        Console.ReadLine();
    }
}

