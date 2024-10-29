using OrderInboxTableConsumerService.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddQuartz(configurator =>
{
    JobKey jobKey = new("StockTransactionJob");
    configurator.AddJob<StockTransactionJob>(options => options.WithIdentity(jobKey));

    TriggerKey triggerKey = new("StockTransactionTrigger");
    configurator.AddTrigger(options => options.ForJob(jobKey)
    .WithIdentity(triggerKey)
    .StartAt(DateTime.UtcNow)
    .WithSimpleSchedule(builder => builder
        .WithIntervalInSeconds(15)
        .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();

