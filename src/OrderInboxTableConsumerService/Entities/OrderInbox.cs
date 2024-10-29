namespace OrderInboxTableConsumerService.Entities
{
    public class OrderInbox
    {
        public Guid IdempotentToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}

