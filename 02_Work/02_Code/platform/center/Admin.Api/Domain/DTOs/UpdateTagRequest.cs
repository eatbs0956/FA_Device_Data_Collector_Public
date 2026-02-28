using System.ComponentModel.DataAnnotations;

namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 更新标签请求
/// </summary>
public class UpdateTagRequest
{
    /// <summary>
    /// 标签标识符（唯一）
    /// </summary>
    [Required(ErrorMessage = "标签标识符不能为空")]
    [MaxLength(128, ErrorMessage = "标签标识符长度不能超过128个字符")]
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称
    /// </summary>
    [Required(ErrorMessage = "标签名称不能为空")]
    [MaxLength(128, ErrorMessage = "标签名称长度不能超过128个字符")]
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 标签地址（JSON格式，存储协议特定的地址配置）
    /// </summary>
    [Required(ErrorMessage = "标签地址不能为空")]
    [MaxLength(256, ErrorMessage = "标签地址长度不能超过256个字符")]
    public string TagAddress { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型
    /// </summary>
    [Required(ErrorMessage = "数据类型不能为空")]
    [MaxLength(32, ErrorMessage = "数据类型长度不能超过32个字符")]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// 单位
    /// </summary>
    [MaxLength(16, ErrorMessage = "单位长度不能超过16个字符")]
    public string? Unit { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 启用状态
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否启用实时推送
    /// </summary>
    public bool EnableRealtime { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 比例因子
    /// </summary>
    public decimal ScalingFactor { get; set; } = 1.0m;

    /// <summary>
    /// 偏移量
    /// </summary>
    public decimal Offset { get; set; } = 0.0m;

    /// <summary>
    /// 访问模式
    /// </summary>
    [MaxLength(16, ErrorMessage = "访问模式长度不能超过16个字符")]
    public string AccessMode { get; set; } = "ReadOnly";

    /// <summary>
    /// 死区值
    /// </summary>
    public decimal Deadband { get; set; } = 0.0m;
}
