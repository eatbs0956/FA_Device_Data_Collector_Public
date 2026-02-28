namespace Auth.Api.Contracts;

/// <summary>
/// 用户数据传输对象 - 用于API响应
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    /// <summary>
    /// 用户类型 - user: 人员账号, service: 服务账号
    /// </summary>
    public string UserType { get; set; } = "user";
    public string? UserGender { get; set; }
    public string UserPhone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int Status { get; set; }
    public string[] UserRoles { get; set; } = Array.Empty<string>();
    
    // 审计字段
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// 角色数据传输对象 - 用于API响应
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleDesc { get; set; } = string.Empty;
    public int Status { get; set; }
    
    // 审计字段
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
