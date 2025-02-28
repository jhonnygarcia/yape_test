using Application.Components.Messaging;
using Application.Components.Messaging.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Application.DbModel;

namespace WebApi
{
    public static class DependencyInjection
    {
        public static void AddAppSettings(this ConfigurationManager configuration)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();
        }

        public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgresConnection");
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            return services;
        }

        public static IServiceCollection AddMessages(this IServiceCollection services, IConfiguration configuration)
        {
            var handlerTypes = typeof(Application.DependencyInjection).Assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaceTypes = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                    .ToList();

                services.AddScoped(handlerType); // Register the handler itself

                foreach (var interfaceType in interfaceTypes)
                {
                    var messageType = interfaceType.GetGenericArguments()[0]; // Extract message type (T)

                    // Register KafkaConsumerService<THandler, TMessage> as HostedService for each message type
                    var hostedServiceType = typeof(KafkaConsumerService<,>).MakeGenericType(handlerType, messageType);

                    var method = typeof(ServiceCollectionHostedServiceExtensions)
                    .GetMethods()
                    .First(m => m.Name == "AddHostedService" && m.IsGenericMethod)
                    .MakeGenericMethod(hostedServiceType);

                    method.Invoke(null, [services]);
                }
            }

            return services;
        }
    }
}
