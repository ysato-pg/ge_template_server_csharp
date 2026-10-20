using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace GeTemplate.Server.Common;

/// <summary>
/// ユーザー ID・トークンの発行と、認証トークンのハッシュ化。
/// クライアントの疑似サーバ（MockServerLogic）と同じ規則にする。
/// </summary>
public static class TokenGenerator
{
    private const int TokenByteLength = 32;

    /// <summary>ユーザー ID（GUID・32 桁の 16 進数）を発行する。</summary>
    public static string NewUserId() => Guid.NewGuid().ToString("N");

    /// <summary>32 バイトの乱数を Base64URL（パディング無し）にしたトークンを発行する。</summary>
    public static string NewToken() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenByteLength));

    /// <summary>認証トークンの SHA-256（16 進数・小文字）。</summary>
    public static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
