namespace Auth.Api.Services.Abstractions;

/// <summary>
/// 密码管理服务接口 - 定义密码加密和验证的抽象契约
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// 哈希密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>哈希后的密码</returns>
    string HashPassword(string password);

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="hashedPassword">哈希后的密码</param>
    /// <param name="providedPassword">提供的密码</param>
    /// <returns>是否匹配</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);

    /// <summary>
    /// 生成随机密码
    /// </summary>
    /// <param name="length">密码长度</param>
    /// <returns>随机密码</returns>
    string GenerateRandomPassword(int length = 12);

    /// <summary>
    /// 验证密码强度
    /// </summary>
    /// <param name="password">密码</param>
    /// <returns>是否符合强度要求</returns>
    bool ValidatePasswordStrength(string password);

    /// <summary>
    /// 获取密码强度等级
    /// </summary>
    /// <param name="password">密码</param>
    /// <returns>强度等级(0-4: 弱-强)</returns>
    int GetPasswordStrength(string password);
}
