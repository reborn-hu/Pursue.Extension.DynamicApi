using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Pursue.Extension.DependencyInjection
{
    public sealed class AutoApiOptions
    {
        public static string DefaultVersion { get; set; } = "";

        /// <summary>
        /// API HTTP动词,默认值 'POST'.
        /// </summary>
        public static string DefaultHttpVerb { get; set; } = "POST";

        /// <summary>
        ///所有API的路由前缀,默认值 'api'.
        /// </summary>
        public static string DefaultApiPreFix { get; set; } = "api";

        /// <summary>
        /// 需要扫描的工程前缀
        /// </summary>
        public static string AssemblyFilterPrefix { get; set; }

        /// <summary>
        /// 删除动态API类（控制器）名称postfix,默认值是{'AppService','ApplicationService'}.
        /// </summary>
        public static List<string> RemoveControllerPostfixes { get; set; } = new List<string>() { "AppService", "ApplicationService" };

        /// <summary>
        /// 删除动态API类的方法（操作）后缀,默认值是{'Async'}.
        /// </summary>
        public static List<string> RemoveActionPostfixes { get; set; } = new List<string>() { "Async" };

        /// <summary>
        ///忽略MVC表单绑定类型。
        /// </summary>
        public static List<Type> FormBodyBindingIgnoredTypes { get; set; } = new List<Type>() { typeof(IFormFile) };

        public static Dictionary<string, string> HttpVerbs { get; private set; } = new Dictionary<string, string>()
        {
            ["add"] = "POST",
            ["create"] = "POST",
            ["post"] = "POST",

            ["get"] = "GET",
            ["find"] = "GET",
            ["fetch"] = "GET",
            ["query"] = "GET",

            ["update"] = "PUT",
            ["put"] = "PUT",

            ["delete"] = "DELETE",
            ["remove"] = "DELETE",
        };

        public AutoApiOptions UseAutoApiOptions(string assemblyFilterPrefix)
        {
            DefaultVersion = string.Empty;
            DefaultHttpVerb = "POST";
            DefaultApiPreFix = "api";
            AssemblyFilterPrefix = assemblyFilterPrefix;
            RemoveControllerPostfixes = new List<string>() { "AppService", "ApplicationService" };
            RemoveActionPostfixes = new List<string>() { "Async" };
            FormBodyBindingIgnoredTypes = new List<Type>() { typeof(IFormFile) };

            return this;
        }

        public AutoApiOptions UseAutoApiOptions(IConfiguration configuration, string section = "AutoApiSettings")
        {
            var dynamicApiConfig = configuration.GetSection(section).Get<AutoApiRoot>();

            if (!dynamicApiConfig.Enable)
            {
                DefaultVersion = string.Empty;
                DefaultHttpVerb = "POST";
                DefaultApiPreFix = "api";
                AssemblyFilterPrefix = "";
                RemoveControllerPostfixes = new List<string>() { "AppService", "ApplicationService" };
                RemoveActionPostfixes = new List<string>() { "Async" };
                FormBodyBindingIgnoredTypes = new List<Type>() { typeof(IFormFile) };
            }
            else
            {
                #region DefaultAreaName

                if (string.IsNullOrEmpty(dynamicApiConfig.DefaultAreaName))
                    DefaultVersion = string.Empty;
                else
                    DefaultVersion = dynamicApiConfig.DefaultAreaName;

                #endregion

                #region DefaultHttpVerb

                if (string.IsNullOrEmpty(dynamicApiConfig.DefaultHttpVerb))
                    throw new ArgumentException($"{nameof(dynamicApiConfig.DefaultHttpVerb)} 不能为空.");
                else
                    DefaultHttpVerb = dynamicApiConfig.DefaultHttpVerb;

                #endregion

                #region DefaultApiPrefix

                if (string.IsNullOrEmpty(dynamicApiConfig.DefaultApiPrefix))
                    DefaultApiPreFix = string.Empty;
                else
                    DefaultApiPreFix = dynamicApiConfig.DefaultApiPrefix;

                #endregion

                #region AssemblyFilterPrefix

                if (string.IsNullOrEmpty(dynamicApiConfig.AssemblyFilterPrefix))
                    AssemblyFilterPrefix = string.Empty;
                else
                    AssemblyFilterPrefix = dynamicApiConfig.AssemblyFilterPrefix;

                #endregion

                #region RemoveControllerPostfixes

                if (dynamicApiConfig.RemoveControllerPostfixes == null)
                    throw new ArgumentException($"{nameof(dynamicApiConfig.RemoveControllerPostfixes)}  不能为空.");
                else
                    RemoveControllerPostfixes = dynamicApiConfig.RemoveControllerPostfixes;

                #endregion

                #region MyRemoveActionPostfixesRegion

                RemoveActionPostfixes = dynamicApiConfig.RemoveActionPostfixes;

                #endregion
            }
            return this;
        }
    }

    public sealed class AutoApiRoot
    {
        /// <summary>
        /// 是否启用配置
        /// <br>
        /// 默认：false
        /// </br>
        /// </summary>s
        public bool Enable { get; set; } = false;

        public string DefaultAreaName { get; set; }

        /// <summary>
        /// API HTTP动词,默认值 'POST'.
        /// </summary>
        public string DefaultHttpVerb { get; set; } = "POST";

        /// <summary>
        ///所有API的路由前缀,默认值 'api'.
        /// </summary>
        public string DefaultApiPrefix { get; set; } = "api";

        /// <summary>
        /// 需要扫描的工程前缀
        /// </summary>
        public string AssemblyFilterPrefix { get; set; }

        /// <summary>
        /// 删除动态API类（控制器）名称postfix,默认值是{'AppService','ApplicationService'}.
        /// </summary>
        public List<string> RemoveControllerPostfixes { get; set; } = new List<string>() { "AppService", "ApplicationService" };

        /// <summary>
        /// 删除动态API类的方法（操作）后缀,默认值是{'Async'}.
        /// </summary>
        public List<string> RemoveActionPostfixes { get; set; } = new List<string>() { "Async" };
    }
}