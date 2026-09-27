namespace Core.Application.Abstractions.Security
{
    /// <summary>
    /// رمزنگاری/رمزگشایی رمز عبور و توکن.
    /// پیاده‌سازی در Infrastructure (DataProtection).
    /// </summary>
    public interface ISecretProtector
    {
        string Protect(string plain);
        string Unprotect(string cipher);
        bool TryUnprotect(string cipher, out string plain);
    }
}