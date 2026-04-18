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
        public DbSet<GitHubRepositorySelection> GitHubRepositorySelections { get; set; } = null!;
        public DbSet<GitHubCommit> GitHubCommits { get; set; } = null!;
        public DbSet<GitHubPullRequest> GitHubPullRequests { get; set; } = null!;
        public DbSet<GitHubPullRequestReview> GitHubPullRequestReviews { get; set; } = null!;
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

            builder.Entity<GitHubRepositorySelection>(entity =>
            {
                entity.HasIndex(g => new { g.UserId, g.RepositoryId }).IsUnique();
                entity.Property(g => g.Name).HasMaxLength(256).IsRequired();
                entity.Property(g => g.FullName).HasMaxLength(512).IsRequired();
            });

            builder.Entity<GitHubCommit>(entity =>
            {
                entity.HasIndex(c => new { c.UserId, c.RepositoryId, c.CommitSha }).IsUnique();
                entity.Property(c => c.RepositoryFullName).HasMaxLength(512).IsRequired();
                entity.Property(c => c.CommitSha).HasMaxLength(128).IsRequired();
                entity.Property(c => c.Message).HasMaxLength(4000).IsRequired();
                entity.Property(c => c.AuthorName).HasMaxLength(256).IsRequired();
                entity.Property(c => c.AuthorEmail).HasMaxLength(512).IsRequired();
                entity.Property(c => c.Url).HasMaxLength(1024).IsRequired();
            });

            builder.Entity<GitHubPullRequest>(entity =>
            {
                entity.HasIndex(pr => new { pr.UserId, pr.RepositoryId, pr.GitHubPullRequestId }).IsUnique();
                entity.Property(pr => pr.RepositoryFullName).HasMaxLength(512).IsRequired();
                entity.Property(pr => pr.Title).HasMaxLength(1024).IsRequired();
                entity.Property(pr => pr.State).HasMaxLength(64).IsRequired();
                entity.Property(pr => pr.AuthorLogin).HasMaxLength(256).IsRequired();
                entity.Property(pr => pr.Url).HasMaxLength(1024).IsRequired();
            });

            builder.Entity<GitHubPullRequestReview>(entity =>
            {
                entity.HasIndex(r => r.GitHubReviewId).IsUnique();
                entity.Property(r => r.RepositoryFullName).HasMaxLength(512).IsRequired();
                entity.Property(r => r.ReviewerLogin).HasMaxLength(256).IsRequired();
                entity.Property(r => r.State).HasMaxLength(64).IsRequired();
                entity.Property(r => r.Url).HasMaxLength(1024).IsRequired();
            });

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.Property(rt => rt.Token).HasMaxLength(512).IsRequired();
            });
        }
    }
}
