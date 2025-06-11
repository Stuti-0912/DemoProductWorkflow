namespace DemoProductworkflow.Models
{
    public class EmailLog
    {
        public int Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int Status { get; set; } // 0 = queued, 1 = processing, 2 = sent, 3 = failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
