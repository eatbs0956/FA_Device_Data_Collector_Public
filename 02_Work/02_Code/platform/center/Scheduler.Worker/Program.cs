using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Host.CreateDefaultBuilder(args)
    .ConfigureLogging(b => b.AddConsole())
    .ConfigureServices((ctx, services) => { /* TODO: add cron, windowing jobs */ })
    .Build()
    .Run();
