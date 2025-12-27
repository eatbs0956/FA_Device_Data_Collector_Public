using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 基础实体类 - 包含审计字段和多租户支持
/// </summary>
/// <remarks>
/// 所有业务实体必须继承此类，确保审计追踪和多租户隔离规范
/// 符合 LLD 1.3.2 审计追踪规范和 1.3.5 多租户数据隔离要求
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>
    /// 创建用户ID - 记录创建者的用户ID
    /// </summary>
    /// <remarks>
    /// 类型: Guid? (可空)
    /// 用途: 审计追踪，标识数据创建者
    /// 自动填充: 由 UnifiedDbContext.SaveChanges 自动从 JWT Token 中提取
    /// </remarks>
    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间 - 记录创建时间（UTC时间戳）
    /// </summary>
    /// <remarks>
    /// 类型: DateTimeOffset (非空)
    /// 默认值: DateTimeOffset.UtcNow
    /// 数据库默认值: CURRENT_TIMESTAMP
    /// </remarks>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 最后更新用户ID - 记录最后修改者的用户ID
    /// </summary>
    /// <remarks>
    /// 类型: Guid? (可空)
    /// 用途: 审计追踪，标识数据最后修改者
    /// 自动填充: 由 UnifiedDbContext.SaveChanges 自动从 JWT Token 中提取
    /// </remarks>
    [Column("updated_by")]
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 最后更新时间 - 记录最后更新时间（UTC时间戳）
    /// </summary>
    /// <remarks>
    /// 类型: DateTimeOffset? (可空)
    /// 自动维护: 由 UnifiedDbContext.SaveChanges 自动更新
    /// </remarks>
    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// 软删除标记 - 逻辑删除标记
    /// </summary>
    /// <remarks>
    /// 类型: bool (非空)
    /// 默认值: false
    /// 业务规则: 
    /// - false: 正常数据
    /// - true: 已删除数据（软删除）
    /// - 业务数据采用软删除，物理数据保持完整
    /// - 全局查询过滤器会自动过滤 DeletedFlag = true 的数据
    /// </remarks>
    [Column("deleted_flag")]
    public bool DeletedFlag { get; set; } = false;

    /// <summary>
    /// 租户ID - 多租户数据隔离标识
    /// </summary>
    /// <remarks>
    /// 类型: string (最大长度64，非空)
    /// 默认值: "t1" (默认租户/系统租户)
    /// 特殊租户:
    /// - "t0": 超级管理员租户（仅超级管理员使用）
    /// - "t1": 默认租户/系统租户（普通用户默认租户）
    /// 多租户隔离策略:
    /// - 普通用户查询: 自动过滤当前租户数据（通过 Global Query Filter）
    /// - 超级管理员查询: 可使用 IgnoreQueryFilters() 查看所有租户数据
    /// 自动填充: 由 UnifiedDbContext.SaveChanges 自动从 JWT Token 中提取
    /// </remarks>
    [Column("tenant_id")]
    [MaxLength(64)]
    public string TenantId { get; set; } = "t1";
}
