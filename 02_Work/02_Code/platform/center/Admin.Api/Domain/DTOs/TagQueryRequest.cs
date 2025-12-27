namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 标签查询请求
/// </summary>
public class TagQueryRequest
{
    /// <summary>
    /// 所属设备ID（必填，用于左侧设备树选择后的查询）
    /// </summary>
    public Guid? DeviceId { get; set; }

    /// <summary>
    /// 标签名称（模糊搜索）
    /// </summary>
    public string? TagName { get; set; }

    /// <summary>
    /// 标签标识符（精确搜索）
    /// </summary>
    public string? TagId { get; set; }

    /// <summary>
    /// 是否启用（精确搜索）
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// 数据类型（精确搜索）
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int Current { get; set; } = 1;

    /// <summary>
    /// 每页条数
    /// </summary>
    public int Size { get; set; } = 20;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// 排序方向（asc/desc）
    /// </summary>
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// 标签列表响应
/// </summary>
public class TagListResponse
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<TagDto> Records { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 每页条数
    /// </summary>
    public int Size { get; set; }
}
