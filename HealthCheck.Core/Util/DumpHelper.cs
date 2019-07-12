using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Serializers;
using RuntimeCodeTest.Core.Entities;
using System;

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
                Type = type.GetFriendlyTypeName() ?? typeof(T).Name,
                Data = data
            };
        }

        private static string CreateDumpName<T>(Type type) => type.GetFriendlyTypeName();
    }
}
