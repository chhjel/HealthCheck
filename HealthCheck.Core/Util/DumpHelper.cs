using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Serializers;
using RuntimeCodeTest.Core.Entities;
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
        /// <param name="serializer">Json serializer implementation</param>
        /// <param name="title">Title of the dump</param>
        public static DataDump Dump<T>(this T obj, IDumpJsonSerializer serializer, string title = null)
        {
            serializer = serializer ?? new DumpNullJsonSerializer();
            var data = CreateDumpData(obj, serializer);
            return CreateDump<T>(obj?.GetType(), title, data);
        }
        
        private static string CreateDumpData<T>(T obj, IDumpJsonSerializer serializer)
        {
            if (obj == null)
            {
                return null;
            }

            return serializer?.Serialize(obj) ?? "";
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
