using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.Reports;
using CodeImpact.Domain.Repositories;
using CodeImpact.Infrastructure.Identity;
using CodeImpact.Infrastructure.Persistence;
using CodeImpact.Infrastructure.Services;
using CodeImpact.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CodeImpact.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettingsSectionName));

            services.AddDbContext<CodeImpactDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<AppUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<CodeImpactDbContext>()
                .AddDefaultTokenProviders();

            services.AddDataProtection();
            services.Configure<GitHubSettings>(configuration.GetSection(GitHubSettingsSectionName));
            services.Configure<OpenAISettings>(configuration.GetSection(OpenAISettingsSectionName));
            services.AddHttpClient<IGitHubService, GitHubService>();
            services.AddHttpClient<ILLMService, OpenAIService>();
            services.AddScoped<IGitHubTokenProtector, GitHubTokenProtector>();
            services.AddScoped<IGitHubAccountRepository, GitHubAccountRepository>();
            services.AddScoped<IGitHubRepositorySelectionRepository, GitHubRepositorySelectionRepository>();
            services.AddScoped<IGitHubCommitRepository, GitHubCommitRepository>();
            services.AddScoped<IGitHubPullRequestRepository, GitHubPullRequestRepository>();
            services.AddScoped<IGitHubPullRequestReviewRepository, GitHubPullRequestReviewRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IBackgroundJobExecutionRepository, BackgroundJobExecutionRepository>();
            services.AddScoped<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
            services.AddScoped<IExecutiveReportExportService, ExecutiveReportExportService>();

            var jwtSettings = configuration.GetSection(JwtSettingsSectionName).Get<JwtSettings>();
            var key = Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? string.Empty);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings?.Issuer,
                        ValidAudience = jwtSettings?.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            services.AddScoped<ITokenService, TokenService>();

            return services;
        }

        public const string JwtSettingsSectionName = "JwtSettings";
        public const string GitHubSettingsSectionName = "GitHub";
        public const string OpenAISettingsSectionName = "OpenAI";
    }
}
