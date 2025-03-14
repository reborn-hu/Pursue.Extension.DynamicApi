using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace Pursue.Extension.DynamicApi
{
    sealed class AutoApiControllerFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            if (!typeof(IEnableAutoApi).IsAssignableFrom(typeInfo.AsType()) || !typeInfo.IsPublic || typeInfo.IsAbstract || typeInfo.IsGenericType && ReflectionExtensions.GetSingleAttributeOrDefaultByFullSearch<AutoApiAttribute>(typeInfo) == null && ReflectionExtensions.GetSingleAttributeOrDefaultByFullSearch<DisableApiAttribute>(typeInfo) != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}