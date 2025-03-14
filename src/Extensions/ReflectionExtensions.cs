using System;
using System.Linq;
using System.Reflection;

namespace Pursue.Extension.DynamicApi
{
    internal static class ReflectionExtensions
    {
        internal static T GetSingleAttributeOrDefaultByFullSearch<T>(TypeInfo info) where T : Attribute
        {
            var attributeType = typeof(T);

            if (info.IsDefined(attributeType, true))
                return info.GetCustomAttributes(attributeType, true).Cast<T>().First();
            else
            {
                foreach (var implInter in info.ImplementedInterfaces)
                {
                    var res = GetSingleAttributeOrDefaultByFullSearch<T>(implInter.GetTypeInfo());

                    if (res != null)
                        return res;
                }
            }
            return null;
        }

        internal static T GetSingleAttributeOrDefault<T>(MemberInfo memberInfo, T defaultValue = default, bool inherit = true) where T : Attribute
        {
            var attributeType = typeof(T);

            if (memberInfo.IsDefined(typeof(T), inherit))
                return memberInfo.GetCustomAttributes(attributeType, inherit).Cast<T>().First();
            return defaultValue;
        }

        internal static T GetSingleAttributeOrNull<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            var attrs = memberInfo.GetCustomAttributes(typeof(T), inherit).ToArray();

            if (attrs.Length > 0)
                return (T)attrs[0];

            return default;
        }

        internal static T GetSingleAttributeOfTypeOrBaseTypesOrNull<T>(this Type type, bool inherit = true) where T : Attribute
        {
            var attr = type.GetTypeInfo().GetSingleAttributeOrNull<T>();
            if (attr != null)
                return attr;

            if (type.GetTypeInfo().BaseType == null)
                return null;

            return type.GetTypeInfo().BaseType.GetSingleAttributeOfTypeOrBaseTypesOrNull<T>(inherit);
        }
    }
}