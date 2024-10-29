using MassTransit;
using Shared.Events;
using Microsoft.Data.SqlClient;
using Dapper;
using OrderCreatedEventHandlerService.Entities;
using System.Text.Json;

namespace OrderCreatedEventHandlerService.Consumers
{
    public class OrderCreatedEventConsumer: IConsumer<OrderCreatedEvent>
    {
        private string _connectionString;

        public OrderCreatedEventConsumer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLServer")!;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var count = await sqlConnection.ExecuteScalarAsync<int>("Select Count(1) From OrderInboxes Where IdempotentToken = @Token", new {Token = context.Message.IdempotentToken});

                if (count == 1) return;

                var newOrderInbox = new OrderInbox
                {
                    IdempotentToken = context.Message.IdempotentToken,
                    CreatedDate = DateTime.UtcNow,
                    ProcessedDate = null,
                    Type = OrderCreatedEvent.GetName(),
                    Payload = JsonSerializer.Serialize(context.Message)
                };

                var insertSql = @"
                                INSERT INTO OrderInboxes (IdempotentToken, CreatedDate, ProcessedDate, Type, Payload)
                                VALUES (@IdempotentToken, @CreatedDate, @ProcessedDate, @Type, @Payload)";

                await sqlConnection.ExecuteAsync(insertSql, newOrderInbox);
            }
        }
    }
}

