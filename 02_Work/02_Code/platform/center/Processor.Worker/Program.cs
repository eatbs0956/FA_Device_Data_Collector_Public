using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Host.CreateDefaultBuilder(args)
    .ConfigureLogging(b => b.AddConsole())
    .ConfigureServices((ctx, services) => { /* TODO: add RabbitMQ consumer, Influx writers */ })
    .Build()
    .Run();
