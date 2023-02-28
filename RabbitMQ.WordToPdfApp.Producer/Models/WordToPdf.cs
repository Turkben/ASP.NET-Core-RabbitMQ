namespace RabbitMQ.WordToPdfApp.Producer.Models
{
    public class WordToPdf
    {
        public string Email { get; set; }
        public IFormFile File { get; set; }
    }
}
