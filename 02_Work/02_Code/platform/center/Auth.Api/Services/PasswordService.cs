using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services;

/// <summary>
/// 密码服务 - 负责密码策略验证、哈希处理等密码相关功能
/// </summary>
public class PasswordService
{
    /// <summary>
    /// 验证密码策略 - 检查密码是否符合安全要求（第一阶段策略：最少8位，至少包含4类字符中的3类）
    /// </summary>
    /// <param name="password">待验证密码 - 用户输入的密码字符串</param>
    /// <param name="msg">错误消息 - 当验证失败时输出的错误描述</param>
    /// <returns>验证结果 - true表示密码符合策略，false表示不符合</returns>
    public bool ValidatePolicy(string password, out string msg)
    {
        // 消息初始化 - 清空输出消息
        msg = string.Empty;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            msg = "密码至少8位";
            return false;
        }
        // 字符类型计数器 - 统计密码中包含的字符类型数量
        int kinds = 0;
        if (Regex.IsMatch(password, "[a-z]")) kinds++;
        if (Regex.IsMatch(password, "[A-Z]")) kinds++;
        if (Regex.IsMatch(password, "[0-9]")) kinds++;
        if (Regex.IsMatch(password, "[^a-zA-Z0-9]")) kinds++;
        if (kinds < 3)
        {
            msg = "密码需包含大小写字母/数字/符号中至少三类";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 哈希密码 - 使用 PBKDF2 算法和 HMAC-SHA256 对密码进行安全哈希处理
    /// </summary>
    /// <param name="password">原始密码 - 需要哈希的明文密码</param>
    /// <returns>哈希结果 - 包含盐值和哈希值的 Base64 编码字符串</returns>
    public string Hash(string password)
    {
        // PBKDF2 with HMACSHA256
        // 随机数生成器 - 用于生成密码盐值
        using var rng = RandomNumberGenerator.Create();
        // 盐值缓冲区 - 16字节的随机盐值存储空间
        Span<byte> salt = stackalloc byte[16];
        rng.GetBytes(salt);
        // 密码哈希计算 - 使用 PBKDF2 进行 100,000 次迭代的哈希计算
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    /// <summary>
    /// 验证密码 - 将输入密码与存储的哈希值进行安全比较验证
    /// </summary>
    /// <param name="password">输入密码 - 用户提供的明文密码</param>
    /// <param name="hash">存储哈希 - 数据库中存储的密码哈希值</param>
    /// <returns>验证结果 - true表示密码正确，false表示密码错误</returns>
    public bool Verify(string password, string hash)
    {
        // 哈希分割 - 将存储的哈希字符串分割为盐值和哈希值部分
        var parts = hash.Split(':');
        if (parts.Length != 2) return false;
        // 盐值解码 - 从 Base64 编码恢复原始盐值
        var salt = Convert.FromBase64String(parts[0]);
        // 期望哈希值 - 从存储中获取的哈希值
        var expected = Convert.FromBase64String(parts[1]);
        // 实际哈希计算 - 使用相同参数对输入密码进行哈希
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, expected.Length);
        // 时间安全比较 - 使用固定时间比较防止时序攻击
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
