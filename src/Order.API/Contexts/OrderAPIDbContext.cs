using Microsoft.EntityFrameworkCore;

namespace Order.API.Contexts
{
    public class OrderAPIDbContext : DbContext
    {
        public OrderAPIDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Order.API.Entities.Order> Orders { get; set; }
        public DbSet<Order.API.Entities.OrderItem> OrderItems { get; set; }
        public DbSet<Order.API.Entities.OrderOutbox> OrderOutboxes { get; set; }

    }
}

