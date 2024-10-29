using Microsoft.Data.SqlClient;
using Quartz;
using Dapper;
using OrderInboxTableConsumerService.Entities;

namespace OrderInboxTableConsumerService.Jobs
{
    public class StockTransactionJob : IJob
    {
        private readonly string _connectionString;

        public StockTransactionJob(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLServer")!;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var unProcessed = await sqlConnection.QueryAsync<OrderInbox>(@"Select * From OrderInboxes Where ProcessedDate is null");

                foreach (var item in unProcessed)
                {
                    //işlemler
                    await sqlConnection.ExecuteAsync(@"Update OrderInboxes Set ProcessedDate = GetDate() Where IdempotentToken = @Token", new { Token = item.IdempotentToken });
                }
            }
        }
    }
}