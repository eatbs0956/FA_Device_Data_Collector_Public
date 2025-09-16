using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Net.Client;
using daq;
using Google.Protobuf.WellKnownTypes;

public class CollectorService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // gRPC channel to ingestion (assume ingestion runs on localhost:5001)
        using var channel = GrpcChannel.ForAddress("http://localhost:5001");
        var client = new Ingestion.IngestionClient(channel);

        using var call = client.Upload(); // client streaming

        while (!stoppingToken.IsCancellationRequested)
        {
            // Here you would read from actual device adapters (serial/tcp/opcua)
            var dp = new DataPoint
            {
                DeviceId = "DEV001",
                PointId = "P001",
                Value = new System.Random().NextDouble() * 100,
                Ts = Timestamp.FromDateTime(System.DateTime.UtcNow)
            };

            await call.RequestStream.WriteAsync(dp);

            await Task.Delay(1000, stoppingToken);
        }

        await call.RequestStream.CompleteAsync();
        var ack = await call.ResponseAsync;
    }
}
