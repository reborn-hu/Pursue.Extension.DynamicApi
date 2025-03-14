using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Pursue.Extension.DynamicApi;
using System;
using System.Linq;

namespace Pursue.Extension.DependencyInjection
{
    public static class AutoApiDependencyInjection
    {
        /// <summary>
        /// 将动态WebApi添加到容器
        /// </summary>
        /// <param name="services">DI服务</param>
        /// <param name="options">配置文件</param>
        /// <returns></returns>
        public static IServiceCollection AddAutoApi(this IServiceCollection services, Action<AutoApiOptions> options)
        {
            options.Invoke(new AutoApiOptions());

            var partManager = services.GetSingletonInstanceOrNull<ApplicationPartManager>() ?? throw new InvalidOperationException("\"AddAutoApi\" 必须 \"AddMvc\" 在之后 .");
            if (!string.IsNullOrEmpty(AutoApiOptions.AssemblyFilterPrefix))
            {
                partManager.ApplicationParts.Where(o => !o.Name.StartsWith(AutoApiOptions.AssemblyFilterPrefix)).ToList().ForEach(item =>
                {
                    partManager.ApplicationParts.Remove(item);
                });
            }
            partManager.FeatureProviders.Add(new AutoApiControllerFeatureProvider());
            services.Configure<MvcOptions>(o =>
            {
                o.Conventions.Add(new AutoApiConvention());
            });

            return services;
        }
    }
}