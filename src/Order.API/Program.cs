using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;
using Order.API.Entities;
using Order.API.ViewModels;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderAPIDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create", async (OrderCreateVM orderCreateVM, OrderAPIDbContext dbContext) =>
{
    var utcNow = DateTime.UtcNow;

    await dbContext.Database.BeginTransactionAsync();

    var orderId = Guid.NewGuid();
    await dbContext.AddAsync<Order.API.Entities.Order>(new()
    {
        Id = orderId,
        BuyerId = orderCreateVM.BuyerId,
        CreatedDate = utcNow,
        OrderItems = orderCreateVM.OrderCreateItems.Select(x => new Order.API.Entities.OrderItem
        {
            ProductId = x.ProductId,
            Count = x.Count,
            Price = x.Price
        }).ToList()
    });

    var idempotentToken = Guid.NewGuid();

    OrderCreatedEvent orderCreatedEvent = new()
    {
        IdempotentToken = idempotentToken,
        OrderId = orderId,
        OrderItems = orderCreateVM.OrderCreateItems.Select(x => new Shared.Models.OrderItemModel
        {
            ProductId = x.ProductId,
            Count = x.Count
        }).ToList()
    };

    await dbContext.AddAsync<OrderOutbox>(new()
    {
        IdempotentToken = idempotentToken,
        CreatedDate = utcNow,
        Type = OrderCreatedEvent.GetName(),
        Payload = JsonSerializer.Serialize(orderCreatedEvent)
    });

    await dbContext.SaveChangesAsync();
    await dbContext.Database.CommitTransactionAsync();

});

app.Run();

