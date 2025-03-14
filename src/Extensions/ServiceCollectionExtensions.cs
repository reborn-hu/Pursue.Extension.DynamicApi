using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Pursue.Extension.DynamicApi
{
    internal static class ServiceCollectionExtensions
    {
        internal static T GetSingletonInstance<T>(this IServiceCollection services)
        {
            var service = services.GetSingletonInstanceOrNull<T>();

            if (service == null)
                throw new InvalidOperationException("找不到Singleton服务: " + typeof(T).AssemblyQualifiedName);

            return service;
        }

        internal static T GetSingletonInstanceOrNull<T>(this IServiceCollection services)
        {
            return (T)services.FirstOrDefault(d => d.ServiceType == typeof(T))?.ImplementationInstance;
        }

        internal static bool IsAdded<T>(this IServiceCollection services)
        {
            return services.IsAdded(typeof(T));
        }

        internal static bool IsAdded(this IServiceCollection services, Type type)
        {
            return services.Any(d => d.ServiceType == type);
        }

        internal static IServiceProvider BuildServiceProviderFromFactory(this IServiceCollection services)
        {
            foreach (var service in services)
            {
                var factoryInterface = service.ImplementationInstance?.GetType()
                    .GetTypeInfo()
                    .GetInterfaces()
                    .FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IServiceProviderFactory<>));

                if (factoryInterface == null)
                    continue;

                var containerBuilderType = factoryInterface.GenericTypeArguments[0];

                return (IServiceProvider)typeof(ServiceCollectionExtensions)
                    .GetTypeInfo()
                    .GetMethods()
                    .Single(m => m.Name == nameof(BuildServiceProviderFromFactory) && m.IsGenericMethod)
                    .MakeGenericMethod(containerBuilderType)
                    .Invoke(null, new object[] { services, null });
            }

            return services.BuildServiceProvider();
        }

        internal static IServiceProvider BuildServiceProviderFromFactory<T>(this IServiceCollection services, Action<T> builderAction = null)
        {
            var serviceProviderFactory = services.GetSingletonInstanceOrNull<IServiceProviderFactory<T>>();

            if (serviceProviderFactory == null)
                throw new ArgumentNullException($"找不到 {typeof(IServiceProviderFactory<T>).FullName} in {services}.");

            var builder = serviceProviderFactory.CreateBuilder(services);

            builderAction?.Invoke(builder);

            return serviceProviderFactory.CreateServiceProvider(builder);
        }
    }
}