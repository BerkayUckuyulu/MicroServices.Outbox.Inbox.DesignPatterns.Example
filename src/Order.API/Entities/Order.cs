namespace Order.API.Entities;

public class Order
{
    public Guid Id { get; set; }
    public int BuyerId { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}