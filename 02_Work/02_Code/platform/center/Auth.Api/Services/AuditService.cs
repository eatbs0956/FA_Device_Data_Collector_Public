using System.Threading.Channels;
using Shared.Domain.Data;
using Shared.Domain.Entities;
using Auth.Api.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services;

/// <summary>
/// 异步审计服务 - 使用后台队列处理审计日志写入
/// </summary>
/// <remarks>
/// 设计要点：
/// 1. 使用 Channel 作为高性能无界队列
/// 2. 后台 HostedService 消费队列并批量写入数据库
/// 3. 不阻塞主请求流程，提升系统吞吐量
/// 4. 异常隔离，审计失败不影响业务
/// </remarks>
public class AuditService : BackgroundService, Abstractions.IAuditService
{
    private readonly Channel<AuditLogEntry> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditService> _logger;

    // 批处理配置
    private const int BatchSize = 50; // 每批最多写入50条
    private const int BatchDelayMs = 1000; // 最长等待1秒触发批量写入

    public AuditService(IServiceProvider serviceProvider, ILogger<AuditService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // 创建无界通道（生产环境可配置有界通道防止内存溢出）
        _channel = Channel.CreateUnbounded<AuditLogEntry>(new UnboundedChannelOptions
        {
            SingleReader = true, // 只有一个后台任务读取
            SingleWriter = false  // 多个请求线程可能同时写入
        });

        _logger.LogInformation("异步审计服务已初始化 - 批大小={BatchSize}, 批延迟={BatchDelayMs}ms", 
            BatchSize, BatchDelayMs);
    }

    /// <summary>
    /// 将审计日志排队（非阻塞，立即返回）
    /// </summary>
    public void EnqueueAuditLog(AuditLogEntry entry)
    {
        if (!_channel.Writer.TryWrite(entry))
        {
            // 理论上无界通道不会失败，除非通道已关闭
            _logger.LogWarning("审计日志入队失败，可能服务正在关闭");
        }
    }

    /// <summary>
    /// 获取队列大小
    /// </summary>
    public int GetQueueSize()
    {
        return _channel.Reader.Count;
    }

    /// <summary>
    /// 后台任务执行方法 - 批量消费队列
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("审计日志后台处理任务已启动");

        await foreach (var batch in ReadBatchesAsync(stoppingToken))
        {
            await ProcessBatchAsync(batch, stoppingToken);
        }

        _logger.LogInformation("审计日志后台处理任务已停止");
    }

    /// <summary>
    /// 批量读取队列数据
    /// </summary>
    private async IAsyncEnumerable<List<AuditLogEntry>> ReadBatchesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var batch = new List<AuditLogEntry>(BatchSize);

        while (!cancellationToken.IsCancellationRequested)
        {
            List<AuditLogEntry>? batchToReturn = null;

            try
            {
                // 尝试读取一条数据（如果队列为空则等待）
                var hasData = await _channel.Reader.WaitToReadAsync(cancellationToken);
                if (!hasData) break; // 通道已关闭且无数据

                // 读取第一条
                if (_channel.Reader.TryRead(out var firstEntry))
                {
                    batch.Add(firstEntry);

                    // 尝试读取更多数据填满批次（非阻塞）
                    while (batch.Count < BatchSize && _channel.Reader.TryRead(out var entry))
                    {
                        batch.Add(entry);
                    }

                    // 准备返回当前批次
                    batchToReturn = new List<AuditLogEntry>(batch);
                    batch.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                // 正常关闭
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "审计日志批处理读取异常");
                
                try
                {
                    await Task.Delay(1000, cancellationToken); // 异常后短暂延迟
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            // 在 try-catch 外部返回批次
            if (batchToReturn != null && batchToReturn.Count > 0)
            {
                yield return batchToReturn;
            }
        }

        // 处理剩余数据
        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    /// <summary>
    /// 处理一个批次的审计日志
    /// </summary>
    private async Task ProcessBatchAsync(List<AuditLogEntry> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();

            // 转换为实体对象
            var auditLogs = batch.Select(entry => new AuditLog
            {
                TenantId = entry.TenantId ?? "t0", // 默认租户
                UserId = entry.UserId,
                Action = entry.Action,
                ResourceType = entry.ResourceType,
                ResourceId = entry.ResourceId,
                IpAddress = entry.IpAddress,
                UserAgent = entry.UserAgent,
                RequestBody = entry.RequestBody,
                ResponseStatus = entry.ResponseStatus,
                ErrorMessage = entry.ErrorMessage,
                CreatedAt = entry.Timestamp
            }).ToList();

            // 批量插入
            db.AuditLogs.AddRange(auditLogs);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("成功写入 {Count} 条审计日志", batch.Count);
        }
        catch (Exception ex)
        {
            // 审计失败不影响主流程，但需要记录错误
            _logger.LogError(ex, "批量写入审计日志失败 - 批大小: {Count}", batch.Count);
        }
    }

    /// <summary>
    /// 服务停止时的清理工作
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("审计服务正在停止，等待队列清空...");

        // 标记通道完成，不再接受新数据
        _channel.Writer.Complete();

        await base.StopAsync(cancellationToken);

        _logger.LogInformation("审计服务已停止");
    }
}
