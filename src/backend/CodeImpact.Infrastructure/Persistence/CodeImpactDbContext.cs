using System;
using CodeImpact.Domain.Entities;
using CodeImpact.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence
{
    public class CodeImpactDbContext : IdentityDbContext<AppUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
    {
        public CodeImpactDbContext(DbContextOptions<CodeImpactDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<UserOrganization> UserOrganizations { get; set; } = null!;
        public DbSet<GitHubAccount> GitHubAccounts { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserOrganization>(entity =>
            {
                entity.HasKey(uo => uo.Id);
                entity.HasIndex(uo => new { uo.UserId, uo.OrganizationId }).IsUnique();
            });

            builder.Entity<GitHubAccount>(entity =>
            {
                entity.HasIndex(g => g.GitHubUserId).IsUnique();
                entity.Property(g => g.GitHubUsername).HasMaxLength(256).IsRequired();
                entity.Property(g => g.EncryptedAccessToken).HasMaxLength(2048).IsRequired();
            });

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.Property(rt => rt.Token).HasMaxLength(512).IsRequired();
            });
        }
    }
}
