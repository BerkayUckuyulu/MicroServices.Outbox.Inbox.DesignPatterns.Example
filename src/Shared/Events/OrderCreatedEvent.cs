using Shared.Models;

namespace Shared.Events
{
    public class OrderCreatedEvent
    {
        public Guid IdempotentToken { get; set; }
        public Guid OrderId { get; set; }
        public List<OrderItemModel> OrderItems { get; set; }

        public static string GetName() => "order_created";
    }
}