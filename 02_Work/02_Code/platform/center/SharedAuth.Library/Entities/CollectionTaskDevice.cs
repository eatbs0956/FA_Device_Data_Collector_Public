using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 采集任务-设备关联实体（多对多关系中间表）
/// </summary>
/// <remarks>
/// 对应LLD文档中的 collection_task_devices 表
/// 实现采集任务与设备的多对多关联
/// 一个任务可以关联多个设备，一个设备可以被多个任务关联
/// </remarks>
[Table("collection_task_devices")]
public class CollectionTaskDevice
{
    /// <summary>
    /// 采集任务ID（外键）
    /// </summary>
    [Required]
    [Column("task_id")]
    public Guid TaskId { get; set; }

    /// <summary>
    /// 设备ID（外键）
    /// </summary>
    [Required]
    [Column("device_id")]
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 导航属性 - 关联的采集任务
    /// </summary>
    [ForeignKey("TaskId")]
    public virtual CollectionTask? Task { get; set; }

    /// <summary>
    /// 导航属性 - 关联的设备
    /// </summary>
    [ForeignKey("DeviceId")]
    public virtual Device? Device { get; set; }
}
