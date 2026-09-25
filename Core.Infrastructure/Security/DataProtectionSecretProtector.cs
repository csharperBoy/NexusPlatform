using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.DataProtection;


namespace Core.Infrastructure.Security
{
    public class DataProtectionSecretProtector : ISecretProtector
        {
            private readonly IDataProtector _protector;

            public DataProtectionSecretProtector(IDataProtectionProvider provider)
            {
                _protector = provider.CreateProtector("NexusPlatform.Secrets.v1");
            }

            public string Protect(string plain)
            {
                if (string.IsNullOrEmpty(plain)) return string.Empty;
                return _protector.Protect(plain);
            }

            public string Unprotect(string cipher)
            {
                if (string.IsNullOrEmpty(cipher)) return string.Empty;
                return _protector.Unprotect(cipher);
            }

            public bool TryUnprotect(string cipher, out string plain)
            {
                plain = string.Empty;
                if (string.IsNullOrEmpty(cipher)) return false;

                try
                {
                    plain = _protector.Unprotect(cipher);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    
}
