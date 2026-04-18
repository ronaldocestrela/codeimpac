using CodeImpact.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CodeImpact.Infrastructure.Services
{
    public class GitHubTokenProtector : IGitHubTokenProtector
    {
        private readonly IDataProtector _protector;

        public GitHubTokenProtector(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector("GitHubTokenProtector");
        }

        public string Protect(string value)
        {
            return _protector.Protect(value);
        }

        public string Unprotect(string value)
        {
            return _protector.Unprotect(value);
        }
    }
}
