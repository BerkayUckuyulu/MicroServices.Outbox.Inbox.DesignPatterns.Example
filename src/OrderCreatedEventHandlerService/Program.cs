using MassTransit;
using OrderCreatedEventHandlerService.Consumers;
using Shared.Constants;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:UserName"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
        _configure.ReceiveEndpoint(RabbitMQQueueNames.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

var host = builder.Build();
host.Run();