using Application.Components.Messaging;
using Application.Components.Messaging.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.Options));
            services.AddSingleton<IMessagingService, KafkaMessagingService>();

            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
            });
            return services;
        }
    }
}
