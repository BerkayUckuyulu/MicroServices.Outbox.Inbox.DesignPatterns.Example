using System.Text.Json;
using Dapper;
using MassTransit;
using Microsoft.Data.SqlClient;
using OrderOutboxTableConsumerService.Entities;
using Quartz;
using Shared.Events;

namespace OrderOutboxTableConsumerService.Jobs
{
    public class OrderCreatedEventPublishJob : IJob
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly string _connectionString;

        public OrderCreatedEventPublishJob(IPublishEndpoint publishEndpoint, IConfiguration configuration)
        {
            _publishEndpoint = publishEndpoint;
            _connectionString = configuration.GetConnectionString("SQLServer")!;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var unProcesseds = await sqlConnection.QueryAsync<OrderOutbox>(
                    "SELECT * FROM OrderOutboxes WHERE ProcessedDate IS NULL AND Type = @Type",
                    new { Type = OrderCreatedEvent.GetName() }
                );
                List<string> updateList = new();

                foreach (var unProcessed in unProcesseds)
                {
                    await _publishEndpoint.Publish(JsonSerializer.Deserialize<OrderCreatedEvent>(unProcessed.Payload));

                    updateList.Add(unProcessed.IdempotentToken.ToString());
                }

                if (updateList.Any())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("Tokens", updateList);

                    await sqlConnection.ExecuteAsync(
                        @"UPDATE OrderOutboxes 
                      SET ProcessedDate = GETDATE() 
                      WHERE IdempotentToken IN @Tokens",
                                        parameters);
                }
            }
        }
    }
}

