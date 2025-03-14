using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pursue.Extension.DynamicApi
{
    internal static class MethodsExtensions
    {
        internal static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        internal static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count <= 0;
        }

        internal static bool IsIn(this string str, params string[] data)
        {
            foreach (var item in data)
            {
                if (str == item)
                    return true;
            }
            return false;
        }

        internal static string RemovePostFix(this string str, params string[] postFixes)
        {
            if (str == null)
                return null;

            if (str == string.Empty)
                return string.Empty;

            if (postFixes.IsNullOrEmpty())
                return str;

            foreach (var postFix in postFixes)
            {
                if (str.EndsWith(postFix))
                    return str.Left(str.Length - postFix.Length);
            }
            return str;
        }

        internal static string RemovePreFix(this string str, params string[] preFixes)
        {
            if (str == null)
                return null;

            if (str == string.Empty)
                return string.Empty;

            if (preFixes.IsNullOrEmpty())
                return str;

            foreach (var preFix in preFixes)
            {
                if (str.StartsWith(preFix))
                    return str.Right(str.Length - preFix.Length);
            }
            return str;
        }

        internal static string Left(this string str, int len)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length < len)
                throw new ArgumentException("len参数不能大于给定字符串的长度！");

            return str.Substring(0, len);
        }

        internal static string Right(this string str, int len)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length < len)
                throw new ArgumentException("len参数不能大于给定字符串的长度！");

            return str.Substring(str.Length - len, len);
        }

        internal static string GetCamelCaseFirstWord(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length == 1)
                return str;

            var res = Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})");

            return res.Length < 1 ? str : res[0];
        }

        internal static string GetPascalCaseFirstWord(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length == 1)
                return str;

            var res = Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})");

            return res.Length < 2 ? str : res[1];
        }

        internal static string GetPascalOrCamelCaseFirstWord(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length == 1)
                return str;
            if (str[0] >= 65 && str[0] <= 90)
                return str.GetPascalCaseFirstWord();
            else
                return str.GetCamelCaseFirstWord();
        }
    }
}