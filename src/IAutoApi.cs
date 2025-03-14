using System;

namespace Pursue.Extension.DynamicApi
{
    /// <summary>
    /// 启用自动API
    /// </summary>
    public interface IEnableAutoApi
    {
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class AutoApiAttribute : Attribute
    {
        public string Version { get; set; } = "";
    }

    /// <summary>
    /// 禁用自动API
    /// </summary>
    public interface IDisableAutoApi
    {
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class DisableApiAttribute : Attribute
    {
    }
}