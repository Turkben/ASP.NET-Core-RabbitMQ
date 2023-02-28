namespace RabbitMQ.WordToPdfApp.Producer.Models
{
    public class WordToPdfMessage
    {
        public byte[] WordByte { get; set; }
        public string Email { get; set; }
        public string FileName { get; set; }
    }
}
