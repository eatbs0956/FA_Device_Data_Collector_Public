using System.ComponentModel.DataAnnotations;

namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 设备分组分页查询请求
/// </summary>
public class DeviceGroupQueryRequest
{
    /// <summary>
    /// 当前页码（从1开始）
    /// </summary>
    public int Current { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int Size { get; set; } = 20;

    /// <summary>
    /// 分组名称（模糊搜索）
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 父分组ID（筛选特定父级下的分组，不传则查询所有）
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 是否只查询顶级分组
    /// </summary>
    public bool? TopLevelOnly { get; set; }
}

/// <summary>
/// 设备分组分页响应
/// </summary>
public class DeviceGroupListResponse
{
    /// <summary>
    /// 记录列表
    /// </summary>
    public List<DeviceGroupDto> Records { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int Size { get; set; }
}

/// <summary>
/// 创建设备分组请求
/// </summary>
public class CreateDeviceGroupRequest
{
    /// <summary>
    /// 分组名称（必填）
    /// </summary>
    [Required(ErrorMessage = "分组名称不能为空")]
    [MaxLength(128, ErrorMessage = "分组名称长度不能超过128")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父分组ID（可选，为空表示顶级分组�?
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 同级排序顺序（可选，默认0�?
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 分组描述（可选）
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 更新设备分组请求
/// </summary>
public class UpdateDeviceGroupRequest
{
    /// <summary>
    /// 分组名称（可选）
    /// </summary>
    [MaxLength(128, ErrorMessage = "分组名称长度不能超过128")]
    public string? Name { get; set; }

    /// <summary>
    /// 父分组ID（可选，传null不更新，传空Guid表示移到顶级�?
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 是否更新父分组（配合ParentId使用�?
    /// </summary>
    public bool UpdateParent { get; set; } = false;

    /// <summary>
    /// 同级排序顺序（可选）
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// 分组描述（可选）
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 移动设备分组请求
/// </summary>
public class MoveDeviceGroupRequest
{
    /// <summary>
    /// 目标父分组ID（为空表示移到顶级）
    /// </summary>
    public Guid? TargetParentId { get; set; }

    /// <summary>
    /// 目标排序位置
    /// </summary>
    public int? TargetSortOrder { get; set; }
}
