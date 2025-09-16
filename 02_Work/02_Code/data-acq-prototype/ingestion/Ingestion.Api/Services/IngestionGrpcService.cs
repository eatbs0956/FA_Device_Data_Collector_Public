using Grpc.Core;
using daq;
using Confluent.Kafka;

public class IngestionGrpcService : Ingestion.IngestionBase
{
    private readonly IProducer<string, string> _producer;

    public IngestionGrpcService()
    {
        var config = new ProducerConfig { BootstrapServers = "kafka:9092" };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public override async Task<UploadAck> Upload(IAsyncStreamReader<DataPoint> requestStream, ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            var dp = requestStream.Current;
            // 简单示例：把点数据序列化为 JSON 发到 Kafka
            var json = System.Text.Json.JsonSerializer.Serialize(new { dp.DeviceId, dp.PointId, dp.Value, ts = dp.Ts });
            await _producer.ProduceAsync("data-points", new Message<string, string> { Key = dp.DeviceId, Value = json });
        }

        return new UploadAck { Ok = true, Message = "Received" };
    }
}
