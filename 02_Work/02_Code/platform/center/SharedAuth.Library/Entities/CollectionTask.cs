using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 采集任务实体 - 管理数据采集任务的配置和状态
/// </summary>
/// <remarks>
/// 对应LLD文档 11.1.1 核心数据模型中的 collection_tasks 表
/// 支持周期采集、定时执行、事件触发、混合模式四种任务类型
/// 继承自 BaseEntity，自动包含审计字段和多租户支持
/// </remarks>
[Table("collection_tasks")]
public class CollectionTask : BaseEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 任务名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 任务编码（唯一标识，可选）
    /// </summary>
    [MaxLength(50)]
    [Column("code")]
    public string? Code { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    [MaxLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    /// <remarks>
    /// Periodic: 周期任务 - 按固定间隔轮询采集
    /// Scheduled: 定时执行 - 按Cron表达式定时执行
    /// EventDriven: 事件触发 - 设备主动上报数据
    /// Hybrid: 混合模式 - 周期轮询 + 事件触发并行
    /// </remarks>
    [Required]
    [MaxLength(20)]
    [Column("task_type")]
    public string TaskType { get; set; } = "Periodic";

    /// <summary>
    /// 采集间隔（毫秒）
    /// </summary>
    /// <remarks>
    /// 仅 Periodic 和 Hybrid 类型需要配置
    /// 范围: 100 - 3600000 (100ms - 1小时)
    /// </remarks>
    [Column("default_interval")]
    public int? DefaultInterval { get; set; }

    /// <summary>
    /// Cron表达式
    /// </summary>
    /// <remarks>
    /// 仅 Scheduled 类型需要配置
    /// 格式: 秒 分 时 日 月 周
    /// 示例: "0 0 2 * * ?" 表示每天凌晨2点执行
    /// </remarks>
    [MaxLength(100)]
    [Column("cron_expression")]
    public string? CronExpression { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    /// <remarks>
    /// 范围: 0-9，数值越大优先级越高
    /// 默认值: 5
    /// </remarks>
    [Column("priority")]
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 任务状态
    /// </summary>
    /// <remarks>
    /// Draft: 草稿 - 未启用
    /// Active: 运行中 - 正常执行采集
    /// Paused: 已暂停 - 暂停执行
    /// Stopped: 已停止 - 完全停止
    /// </remarks>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// 启用状态
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 生效开始时间（可选）
    /// </summary>
    /// <remarks>
    /// 支持定时启动任务
    /// 为空表示立即生效
    /// </remarks>
    [Column("effective_from")]
    public DateTimeOffset? EffectiveFrom { get; set; }

    /// <summary>
    /// 生效结束时间（可选）
    /// </summary>
    /// <remarks>
    /// 支持任务自动停止
    /// 为空表示永久有效
    /// </remarks>
    [Column("effective_to")]
    public DateTimeOffset? EffectiveTo { get; set; }

    /// <summary>
    /// 导航属性 - 关联的设备（多对多）
    /// </summary>
    public virtual ICollection<CollectionTaskDevice> TaskDevices { get; set; } = new List<CollectionTaskDevice>();
}
