using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddGrpc();
// 注册 Kafka producer helper 等

var app = builder.Build();

app.MapGrpcService<IngestionGrpcService>();
app.MapGet("/", () => "Ingestion gRPC Service");

app.Run();
