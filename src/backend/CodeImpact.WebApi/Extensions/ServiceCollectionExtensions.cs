using CodeImpact.Application;
using CodeImpact.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeImpact.WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationServices();
            services.AddInfrastructureServices(configuration);
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }
    }
}
