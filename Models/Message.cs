namespace CaffeBot.Models
{
    public class Message
    {
        public long ChatId { get; set; }
        public string Text { get; set; }
        public IFormFile File { get; set; }
    }
}
