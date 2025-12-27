namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 设备分组数据传输对象
/// </summary>
public class DeviceGroupDto
{
    /// <summary>
    /// 分组ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父分组ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 层级深度
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 同级排序顺序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 分组描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 设备数量（该分组下的直接设备数）
    /// </summary>
    public int DeviceCount { get; set; }

    /// <summary>
    /// 子分组数量
    /// </summary>
    public int ChildCount { get; set; }

    /// <summary>
    /// 子分组列表（树形结构）
    /// </summary>
    public List<DeviceGroupDto> Children { get; set; } = new();

    /// <summary>
    /// 租户ID
    /// </summary>
    public string TenantId { get; set; } = "t1";

    /// <summary>
    /// 创建人ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新人ID
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// 设备分组树节点（简化版，用于下拉选择�?
/// </summary>
public class DeviceGroupTreeNode
{
    /// <summary>
    /// 分组ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父分组ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 层级深度
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 同级排序顺序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 子分组列�?
    /// </summary>
    public List<DeviceGroupTreeNode> Children { get; set; } = new();
}
