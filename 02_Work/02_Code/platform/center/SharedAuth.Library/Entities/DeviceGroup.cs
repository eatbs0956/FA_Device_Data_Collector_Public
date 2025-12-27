using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 设备分组实体 - 业务层级的设备分组管理
/// </summary>
/// <remarks>
/// 与EdgeNode的物理分组互补，提供灵活的业务层级分组
/// 支持树形结构，可无限层级嵌套
/// </remarks>
[Table("device_groups")]
public class DeviceGroup : BaseEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 分组名称
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父分组ID（可选，支持树形结构）
    /// </summary>
    [Column("parent_id")]
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 层级（自动计算）
    /// </summary>
    [Column("level")]
    public int Level { get; set; } = 1;

    /// <summary>
    /// 同级排序顺序（数值越小越靠前）
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 分组描述
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 父分组（导航属性）
    /// </summary>
    [ForeignKey("ParentId")]
    public virtual DeviceGroup? Parent { get; set; }

    /// <summary>
    /// 子分组列表（导航属性）
    /// </summary>
    [InverseProperty("Parent")]
    public virtual ICollection<DeviceGroup> Children { get; set; } = new List<DeviceGroup>();

    /// <summary>
    /// 关联的设备列表（导航属性）
    /// </summary>
    [InverseProperty("Group")]
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
