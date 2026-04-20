using System;
using Microsoft.AspNetCore.Identity;

namespace CodeImpact.Infrastructure.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        public string? ProfileImageUrl { get; set; }

        public string AccountStatus { get; set; } = "Active";

        public string SupportFlagsJson { get; set; } = "[]";
    }
}
