using System.Reflection;
using CodeImpact.Application.AI;
using CodeImpact.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CodeImpact.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddScoped<IContributionPromptBuilder, ContributionPromptBuilder>();
            services.AddScoped<IAIOrchestrator, AIOrchestrator>();
            services.AddScoped<IExecutiveReportOrchestrator, ExecutiveReportOrchestrator>();
            return services;
        }
    }
}
