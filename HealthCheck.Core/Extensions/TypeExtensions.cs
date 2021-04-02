using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace HealthCheck.Core.Extensions
{
    /// <summary>
    /// Extensions related to <see cref="Type"/>s.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Checks if the type is anonymous.
        /// </summary>
        public static bool IsAnonymous(this Type type)
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        /// <summary>
        /// Get a prettier name for the type.
        /// </summary>
        public static string GetFriendlyTypeName(this Type type, Dictionary<string, string> aliases = null)
        {
            if (type == null)
            {
                return null;
            }
            else if (IsAnonymous(type))
            {
                return "AnonymousType";
            }

            var friendlyNameBuilder = new StringBuilder();
            friendlyNameBuilder.Append(type.Name);
            if (type.IsGenericType)
            {
                int iBacktick = friendlyNameBuilder.ToString().IndexOf('`');
                if (iBacktick > 0)
                {
                    var friendlyName = friendlyNameBuilder.ToString().Remove(iBacktick);
                    friendlyNameBuilder.Clear();
                    friendlyNameBuilder.Append(friendlyName);
                }
                friendlyNameBuilder.Append("<");
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyTypeName(typeParameters[i]);
                    typeParamName = ResolveInputTypeAlias(typeParamName, aliases);
                    friendlyNameBuilder.Append(i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyNameBuilder.Append(">");
            }


            var resolvedName = friendlyNameBuilder.ToString();
            return ResolveInputTypeAlias(resolvedName, aliases);
        }

        private static string ResolveInputTypeAlias(string name, Dictionary<string, string> aliases = null)
        {
            if (aliases?.Any() != true)
            {
                return name;
            }
            return aliases.ContainsKey(name) ? aliases[name] : name;
        }
    }
}
