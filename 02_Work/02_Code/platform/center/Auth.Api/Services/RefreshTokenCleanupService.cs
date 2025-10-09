using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services;

/// <summary>
/// 刷新令牌清理服务 - 定期清理过期和已撤销的刷新令牌
/// </summary>
/// <remarks>
/// 该服务作为后台任务运行，定期清理满足以下条件的刷新令牌：
/// 1. 令牌已过期 (ExpiresAt <= NOW)
/// 2. 并且满足以下任一条件：
///    - 令牌已被撤销 (Revoked = true)
///    - 令牌创建时间超过保留期限 (CreatedAt <= NOW - RetentionDays)
/// 
/// 默认配置：
/// - 执行间隔：每24小时执行一次
/// - 保留期限：30天（即使已过期，也保留30天内的记录用于审计）
/// </remarks>
public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly TimeSpan _interval;
    private readonly int _retentionDays;

    /// <summary>
    /// 构造函数 - 初始化刷新令牌清理服务
    /// </summary>
    /// <param name="serviceProvider">服务提供者 - 用于创建作用域获取 DbContext</param>
    /// <param name="logger">日志记录器 - 记录清理操作和错误信息</param>
    public RefreshTokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // 清理间隔 - 每24小时执行一次（可通过环境变量配置）
        var intervalHours = int.TryParse(
            Environment.GetEnvironmentVariable("CLEANUP_INTERVAL_HOURS"), 
            out var hours) ? hours : 24;
        _interval = TimeSpan.FromHours(intervalHours);
        
        // 保留天数 - 默认保留30天内的记录（可通过环境变量配置）
        _retentionDays = int.TryParse(
            Environment.GetEnvironmentVariable("CLEANUP_RETENTION_DAYS"), 
            out var days) ? days : 30;
        
        _logger.LogInformation(
            "刷新令牌清理服务已配置: 执行间隔={IntervalHours}小时, 保留期限={RetentionDays}天",
            intervalHours, _retentionDays);
    }

    /// <summary>
    /// 执行后台任务 - 定期清理过期令牌
    /// </summary>
    /// <param name="stoppingToken">取消令牌 - 用于优雅停止服务</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("刷新令牌清理服务已启动");

        // 首次启动延迟 - 等待1分钟后开始首次清理（避免启动时的性能影响）
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 执行清理操作
                await CleanupExpiredTokensAsync(stoppingToken);
                
                // 等待下一次执行
                _logger.LogInformation(
                    "下次清理将在 {NextRunTime} 执行", 
                    DateTimeOffset.UtcNow.Add(_interval));
                
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // 服务正在停止 - 正常退出
                _logger.LogInformation("刷新令牌清理服务正在停止");
                break;
            }
            catch (Exception ex)
            {
                // 记录错误但继续运行
                _logger.LogError(ex, "清理刷新令牌时发生错误，将在下个周期重试");
                
                // 发生错误后等待较短时间再重试（1小时）
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("刷新令牌清理服务已停止");
    }

    /// <summary>
    /// 清理过期令牌 - 执行实际的数据库清理操作
    /// </summary>
    /// <param name="cancellationToken">取消令牌 - 用于取消操作</param>
    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;
        _logger.LogInformation("开始清理过期刷新令牌...");

        // 创建新的作用域获取 DbContext（避免长时间持有连接）
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var now = DateTimeOffset.UtcNow;
        var cutoffDate = now.AddDays(-_retentionDays); // 计算保留期限截止日期

        try
        {
            // 查询需要删除的令牌
            // 删除条件：已过期 AND (已撤销 OR 创建时间超过保留期限)
            var tokensToDelete = await db.RefreshTokens
                .Where(x => x.ExpiresAt <= now &&
                           (x.Revoked || x.CreatedAt <= cutoffDate))
                .ToListAsync(cancellationToken);

            if (tokensToDelete.Any())
            {
                // 统计信息
                var totalCount = tokensToDelete.Count;
                var revokedCount = tokensToDelete.Count(x => x.Revoked);
                var oldCount = tokensToDelete.Count(x => x.CreatedAt <= cutoffDate);

                // 执行删除
                db.RefreshTokens.RemoveRange(tokensToDelete);
                await db.SaveChangesAsync(cancellationToken);

                var duration = DateTimeOffset.UtcNow - startTime;
                
                _logger.LogInformation(
                    "清理完成: 删除了 {TotalCount} 条过期令牌 " +
                    "(已撤销: {RevokedCount}, 超期: {OldCount}), 耗时: {Duration}ms",
                    totalCount, revokedCount, oldCount, duration.TotalMilliseconds);

                // 记录详细统计信息
                await LogCleanupStatisticsAsync(db, totalCount, cancellationToken);
            }
            else
            {
                _logger.LogInformation("没有需要清理的过期令牌");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行数据库清理操作时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 记录清理统计信息 - 输出当前数据库中令牌的统计情况
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="deletedCount">本次删除的记录数</param>
    /// <param name="cancellationToken">取消令牌</param>
    private async Task LogCleanupStatisticsAsync(
        AuthDbContext db, 
        int deletedCount,
        CancellationToken cancellationToken)
    {
        try
        {
            // 统计当前数据库状态
            var totalRemaining = await db.RefreshTokens.CountAsync(cancellationToken);
            var activeCount = await db.RefreshTokens
                .CountAsync(x => !x.Revoked, cancellationToken);
            var validCount = await db.RefreshTokens
                .CountAsync(x => x.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);

            _logger.LogInformation(
                "数据库状态: 剩余令牌 {Total} 条 (活跃: {Active}, 有效: {Valid}), 本次删除: {Deleted} 条",
                totalRemaining, activeCount, validCount, deletedCount);
        }
        catch (Exception ex)
        {
            // 统计信息失败不影响主流程
            _logger.LogWarning(ex, "记录清理统计信息时发生错误");
        }
    }
}
