using Newtonsoft.Json;
using RuntimeCodeTest.Core.Models;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Helpers for the dump and diff extension methods.
    /// </summary>
    public static class DumpHelper
    {
        /// <summary>
        /// Dump the value of an object to the given list.
        /// </summary>
        /// <param name="obj">Object to dump</param>
        /// <param name="title">Title of the dump</param>
        /// <param name="ignoreErrors">Ignore any properties that fail serialization</param>
        public static DataDump Dump<T>(this T obj, string title = null, bool ignoreErrors = true)
        {
            var data = CreateDumpData(obj, ignoreErrors);
            return CreateDump<T>(obj?.GetType(), title, data);
        }

        /// <summary>
        /// Dump the value of an object to the given list.
        /// </summary>
        /// <param name="obj">Object to dump</param>
        /// <param name="title">Title of the dump</param>
        /// <param name="converters">Any custom <see cref="JsonConverter"></see>s to use.</param>
        public static DataDump Dump<T>(this T obj, String title = null, params JsonConverter[] converters)
        {
            var data = CreateDumpData(obj, false, converters);
            return CreateDump<T>(obj?.GetType(), title, data);
        }

        /// <summary>
        /// Dump the value of an object to the given list.
        /// </summary>
        /// <param name="obj">Object to dump</param>
        /// <param name="dumpConverter">Custom conversion of object to string.</param>
        /// <param name="title">Title of the dump</param>
        public static DataDump Dump<T>(this T obj, Func<T, string> dumpConverter, string title = null)
        {
            var data = dumpConverter(obj);
            return CreateDump<T>(obj?.GetType(), title, data);
        }

        private static string CreateDumpData<T>(this T obj, bool ignoreErrors, params JsonConverter[] converters)
        {
            if (obj == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Error = (sender, e) =>
                {
                    if (ignoreErrors) {
                        sender = null;
                        e.ErrorContext.Handled = true;
                    }
                }
            };

            foreach (var converter in converters)
            {
                settings.Converters.Add(converter);
            }
            return JsonConvert.SerializeObject(obj, settings);
        }

        private static DataDump CreateDump<T>(Type type, string title, string data)
        {
            return new DataDump()
            {
                Title = title ?? CreateDumpName<T>(type),
                Type = GetFriendlyTypeName(type) ?? typeof(T).Name,
                Data = data
            };
        }

        /// <summary>
        /// Checks if the type is anonymous.
        /// </summary>
        public static bool IsAnonymous(Type type)
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        private static string CreateDumpName<T>(Type type) => GetFriendlyTypeName(type);

        private static string GetFriendlyTypeName(Type type)
        {
            if (type == null)
            {
                return null;
            }
            else if (IsAnonymous(type))
            {
                return "AnonymousType";
            }

            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyTypeName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName;
        }
    }
}
