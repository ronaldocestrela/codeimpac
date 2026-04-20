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
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<BackgroundJobExecution> BackgroundJobExecutions { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Plan> Plans { get; set; } = null!;
        public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;
        public DbSet<UserUsageSnapshot> UserUsageSnapshots { get; set; } = null!;
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; } = null!;

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
                entity.Property(g => g.OwnerLogin).HasMaxLength(256).IsRequired();
                entity.Property(g => g.OwnerType).HasMaxLength(64).IsRequired();
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

            builder.Entity<BackgroundJobExecution>(entity =>
            {
                entity.HasIndex(job => new { job.UserId, job.CreatedAt });
                entity.Property(job => job.JobType).HasMaxLength(64).IsRequired();
                entity.Property(job => job.Status).HasMaxLength(64).IsRequired();
                entity.Property(job => job.RequestJson).HasMaxLength(4000).IsRequired();
                entity.Property(job => job.ResultJson).HasMaxLength(16000);
                entity.Property(job => job.ErrorMessage).HasMaxLength(4000);
                entity.Property(job => job.HangfireJobId).HasMaxLength(128);
            });

            builder.Entity<Plan>(entity =>
            {
                entity.HasIndex(plan => plan.Name).IsUnique();
                entity.Property(plan => plan.Name).HasMaxLength(128).IsRequired();
                entity.Property(plan => plan.Description).HasMaxLength(1024).IsRequired();
            });

            builder.Entity<UserSubscription>(entity =>
            {
                entity.HasIndex(subscription => subscription.UserId).IsUnique();
                entity.HasIndex(subscription => new { subscription.Status, subscription.CurrentPeriodEnd });
                entity.Property(subscription => subscription.Status).HasMaxLength(64).IsRequired();
                entity.Property(subscription => subscription.BillingIssue).HasMaxLength(1024);
            });

            builder.Entity<UserUsageSnapshot>(entity =>
            {
                entity.HasIndex(snapshot => snapshot.UserId).IsUnique();
            });

            builder.Entity<AdminAuditLog>(entity =>
            {
                entity.HasIndex(log => new { log.AdminUserId, log.CreatedAt });
                entity.Property(log => log.Action).HasMaxLength(128).IsRequired();
                entity.Property(log => log.TargetType).HasMaxLength(128).IsRequired();
                entity.Property(log => log.TargetId).HasMaxLength(128);
                entity.Property(log => log.PayloadSummary).HasMaxLength(4000).IsRequired();
                entity.Property(log => log.Result).HasMaxLength(64).IsRequired();
                entity.Property(log => log.IpAddress).HasMaxLength(128);
            });

            builder.Entity<Report>(entity =>
            {
                entity.HasIndex(r => new { r.UserId, r.GeneratedAt });
                entity.Property(r => r.DeveloperScope).HasMaxLength(256).IsRequired();
                entity.Property(r => r.RepositoriesJson).HasMaxLength(4000).IsRequired();
                entity.Property(r => r.ExecutiveSummary).HasMaxLength(8000).IsRequired();
                entity.Property(r => r.HighlightsJson).HasMaxLength(8000).IsRequired();
                entity.Property(r => r.RisksJson).HasMaxLength(8000).IsRequired();
                entity.Property(r => r.EvidenceJson).HasMaxLength(16000).IsRequired();
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_Reports_RepositoriesJson_IsJson", "ISJSON([RepositoriesJson]) = 1");
                    table.HasCheckConstraint("CK_Reports_HighlightsJson_IsJson", "ISJSON([HighlightsJson]) = 1");
                    table.HasCheckConstraint("CK_Reports_RisksJson_IsJson", "ISJSON([RisksJson]) = 1");
                    table.HasCheckConstraint("CK_Reports_EvidenceJson_IsJson", "ISJSON([EvidenceJson]) = 1");
                });
            });
        }
    }
}
