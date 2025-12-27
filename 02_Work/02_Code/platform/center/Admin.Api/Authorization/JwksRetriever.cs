using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text.Json;

namespace Admin.Api.Authorization;

/// <summary>
/// JWKS 检索器 - 从 JWKS 端点获取公钥用于 JWT 签名验证
/// </summary>
/// <remarks>
/// 由于 Auth.Api 只提供 /.well-known/jwks.json 端点而没有完整的 OpenID Configuration，
/// 需要自定义 Configuration Retriever 来直接解析 JWKS 格式
/// </remarks>
public class JwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string address,
        IDocumentRetriever retriever,
        CancellationToken cancel)
    {
        Log.Information("JwksRetriever: 开始从 {Address} 获取 JWKS", address);
        
        try
        {
            // 从 JWKS 端点获取 JSON 文档
            var document = await retriever.GetDocumentAsync(address, cancel);
            Log.Information("JwksRetriever: 成功获取 JWKS 文档，长度={Length}", document.Length);
            
            // 解析 JWKS JSON
            var jwks = JsonDocument.Parse(document);
            var keys = new List<SecurityKey>();
            
            // 提取所有密钥
            if (jwks.RootElement.TryGetProperty("keys", out var keysElement))
            {
                var keyCount = 0;
                foreach (var keyElement in keysElement.EnumerateArray())
                {
                    // 提取 RSA 参数
                    if (keyElement.TryGetProperty("kty", out var kty) && kty.GetString() == "RSA")
                    {
                        var n = keyElement.GetProperty("n").GetString();
                        var e = keyElement.GetProperty("e").GetString();
                        var kid = keyElement.TryGetProperty("kid", out var kidProp) ? kidProp.GetString() : null;
                        
                        Log.Information("JwksRetriever: 解析到 RSA 密钥，Kid={Kid}", kid);
                        
                        // 创建 RSA 安全密钥
                        var rsaParameters = new System.Security.Cryptography.RSAParameters
                        {
                            Modulus = Base64UrlEncoder.DecodeBytes(n),
                            Exponent = Base64UrlEncoder.DecodeBytes(e)
                        };
                        
                        var rsa = System.Security.Cryptography.RSA.Create(rsaParameters);
                        var key = new RsaSecurityKey(rsa) { KeyId = kid };
                        keys.Add(key);
                        keyCount++;
                    }
                }
                Log.Information("JwksRetriever: 共解析到 {Count} 个密钥", keyCount);
            }
            else
            {
                Log.Warning("JwksRetriever: JWKS 文档中未找到 'keys' 属性");
            }
            
            // 返回包含密钥的 OpenIdConnectConfiguration
            var config = new OpenIdConnectConfiguration
            {
                Issuer = "devdcp.auth"
            };
            
            // 将密钥添加到配置中
            foreach (var key in keys)
            {
                config.SigningKeys.Add(key);
            }
            
            Log.Information("JwksRetriever: 成功创建 OpenIdConnectConfiguration，包含 {Count} 个签名密钥", config.SigningKeys.Count);
            
            return config;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "JwksRetriever: 获取或解析 JWKS 失败");
            throw;
        }
    }
}
