#if !NETSTANDARD
using HealthCheck.Module.DynamicCodeExecution.Models;
using HealthCheck.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace HealthCheck.Module.DynamicCodeExecution.Util
{
    /// <summary>
    /// Helpers for the dump and diff extension methods.
    /// </summary>
    public static class DumpHelper
    {
#region Diff
        /// <summary>
        /// Creates a dump of the two objects ready for diff consumption in the front-end.
        /// </summary>
        /// <typeparam name="TLeft">Left type</typeparam>
        /// <typeparam name="TRight">Right type</typeparam>
        /// <param name="left">Left object</param>
        /// <param name="right">Right object</param>
        /// <param name="onlyIfDifferent">Only include if different</param>
        /// <param name="list">Internal diff list</param>
        /// <param name="title">Optional title of the dump</param>
        /// <param name="ignoreErrors">Ignore any serialization errors</param>
        public static TLeft Diff<TLeft, TRight>(this TLeft left, TRight right, bool onlyIfDifferent, List<DiffModel> list, string title = null, bool ignoreErrors = true)
        {
            var leftDump = CreateDump<TLeft>(left?.GetType(), null, CreateDumpData(left, ignoreErrors), false);
            var rightDump = CreateDump<TRight>(right?.GetType(), null, CreateDumpData(right, ignoreErrors), false);
            if (onlyIfDifferent && leftDump.Data == rightDump.Data)
            {
                return left;
            }

            list.Add(new DiffModel()
            {
                Title = title ?? $"'{leftDump.Title}' vs '{rightDump.Title}'",
                Left= leftDump,
                Right = rightDump
            });
            return left;
        }
#endregion

        /// <summary>
        /// Dump the value of an object to the given list.
        /// </summary>
        /// <param name="obj">Object to dump</param>
        /// <param name="list">List to add the value to</param>
        /// <param name="title">Title of the dump</param>
        /// <param name="display">Include in result json</param>
        /// <param name="ignoreErrors">Ignore any properties that fail serialization</param>
        public static T Dump<T>(this T obj, List<DataDump> list, string title = null, bool display = true, bool ignoreErrors = true)
        {
            var data = CreateDumpData(obj, ignoreErrors);
            list.Add(CreateDump<T>(obj?.GetType(), title, data, display));
            return obj;
        }

        /// <summary>
        /// Dump the value of an object to the given list.
        /// </summary>
        /// <param name="obj">Object to dump</param>
        /// <param name="list">List to add the value to</param>
        /// <param name="title">Title of the dump</param>
        /// <param name="display">Include in result json</param>
        /// <param name="converters">Any custom <see cref="JsonConverter"></see>s to use.</param>
        public static T Dump<T>(this T obj, List<DataDump> list, string title = null, bool display = true, params JsonConverter[] converters)
        {
            var data = CreateDumpData(obj, false, converters);
            list.Add(CreateDump<T>(obj?.GetType(), title, data, display));
            return obj;
        }

        /// <summary>
        /// Dump the value of an object to the given list.
        /// </summary>
        /// <param name="obj">Object to dump</param>
        /// <param name="list">List to add the value to</param>
        /// <param name="dumpConverter">Custom conversion of object to string.</param>
        /// <param name="title">Title of the dump</param>
        /// <param name="display">Include in result json</param>
        public static T Dump<T>(this T obj, List<DataDump> list, Func<T, string> dumpConverter, string title = null, bool display = true)
        {
            var data = dumpConverter(obj);
            list.Add(CreateDump<T>(obj?.GetType(), title, data, display));
            return obj;
        }

        private static string CreateDumpData<T>(this T obj, bool ignoreErrors, params JsonConverter[] converters)
        {
            if (obj == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings()
            {
                Error = (sender, e) =>
                {
                    if (ignoreErrors) {
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

        /// <summary>
        /// Saves the given list of dumps to the given path.
        /// </summary>
        /// <param name="list">List to save</param>
        /// <param name="pathOrFilename">Path or filename. Defaults to %temp%\DCEDump_%date%.txt</param>
        /// <param name="includeTitle">Include dump title</param>
        /// <param name="includeType">Include dump type</param>
        public static void SaveDumps(List<DataDump> list, string pathOrFilename = null, bool includeTitle = false, bool includeType = false)
        {
            string path = null;
            try
            {
                path = Path.Combine(Path.GetTempPath(), pathOrFilename ?? GetNewDumpFilename());
                var builder = new StringBuilder();
                foreach (var dump in list)
                {
                    if (includeTitle)
                    {
                        builder.AppendLine(dump.Title);
                    }
                    if (includeType)
                    {
                        builder.AppendLine(dump.Type);
                    }
                    builder.AppendLine(dump.Data);
                }
                File.WriteAllText(path, builder.ToString());
                $"Saved dumps to: {path}".Dump(list, "SaveDumps");
            }
            catch (Exception ex)
            {
                $"Tried to save dumps to '{path}' but failed with the message: {ex.Message}".Dump(list, "SaveDumps");
            }
        }

        private static string GetNewDumpFilename()
        {
            var cult = System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag("nb-NO");
            var date = DateTimeOffset.Now.ToString("dd-MM-yyyy_HH-mm-ss", cult);
            return $"DCEDump_{date}.txt";
        }

        private static DataDump CreateDump<T>(Type type, string title, string data, bool display)
        {
            return new DataDump()
            {
                Title = title ?? CreateDumpName(type),
                Type = GetFriendlyTypeName(type) ?? typeof(T).Name,
                Data = data,
                Display = display
            };
        }

        private static string CreateDumpName(Type type) => GetFriendlyTypeName(type);

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

        private static string GetFriendlyTypeName(Type type) => type.GetFriendlyTypeName();
    }
}
#endif
