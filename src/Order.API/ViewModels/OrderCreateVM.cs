namespace Order.API.ViewModels
{
    public class OrderCreateVM
    {
        public int BuyerId { get; set; }
        public List<OrderCreateItemVM> OrderCreateItems { get; set; }
    }
}