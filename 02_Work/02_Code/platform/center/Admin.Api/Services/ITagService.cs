using Admin.Api.Domain.DTOs;

namespace Admin.Api.Services;

/// <summary>
/// 标签服务接口
/// </summary>
public interface ITagService
{
    /// <summary>
    /// 获取标签列表（分页）
    /// </summary>
    Task<TagListResponse> GetTagsAsync(TagQueryRequest request);

    /// <summary>
    /// 根据ID获取标签详情
    /// </summary>
    Task<TagDto?> GetTagByIdAsync(Guid id);

    /// <summary>
    /// 创建标签
    /// </summary>
    Task<Guid> CreateTagAsync(CreateTagRequest request);

    /// <summary>
    /// 更新标签
    /// </summary>
    Task UpdateTagAsync(Guid id, UpdateTagRequest request);

    /// <summary>
    /// 删除标签（软删除）
    /// </summary>
    Task DeleteTagAsync(Guid id);

    /// <summary>
    /// 批量删除标签（软删除）
    /// </summary>
    Task BatchDeleteAsync(List<Guid> ids);

    /// <summary>
    /// 启用/禁用标签
    /// </summary>
    Task ToggleEnabledAsync(Guid id, bool enabled);

    /// <summary>
    /// 批量启用/禁用标签
    /// </summary>
    Task BatchToggleEnabledAsync(List<Guid> ids, bool enabled);

    /// <summary>
    /// 获取设备的所有标签（用于导出）
    /// </summary>
    Task<List<TagDto>> GetTagsByDeviceIdAsync(Guid deviceId);

    /// <summary>
    /// 批量导入标签
    /// </summary>
    Task<int> BatchImportAsync(Guid deviceId, List<CreateTagRequest> tags);
}
