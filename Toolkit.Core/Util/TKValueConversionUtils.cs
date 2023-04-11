using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Exceptions;
using QoDL.Toolkit.Core.Modules.Tests.Services;
using QoDL.Toolkit.Core.Util.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Conversion utils.
    /// </summary>
    public static class TKValueConversionUtils
    {
        internal static TKStringConverter DefaultStringConverter { get; set; } = new TKStringConverter();

        /// <summary>
        /// Convert the given raw input to the given model.
        /// </summary>
        public static T ConvertInputModel<T>(Dictionary<string, string> rawInput, TKStringConverter stringConverter = null,
            Dictionary<string, string> placeholders = null, Func<string, TKCustomPropertyAttribute, Func<string, string>> placeholderTransformerFactory = null)
            where T: class, new()
            => ConvertInputModel(typeof(T), rawInput, stringConverter, placeholders, placeholderTransformerFactory) as T;

        /// <summary>
        /// Convert the given raw input to the given model.
        /// </summary>
        public static T ConvertInputModel<T>(Func<string, string> rawInputGetter, TKStringConverter stringConverter = null,
            Dictionary<string, string> placeholders = null, Func<string, TKCustomPropertyAttribute, Func<string, string>> placeholderTransformerFactory = null)
            where T : class, new()
            => ConvertInputModel(typeof(T), rawInputGetter, stringConverter, placeholders, placeholderTransformerFactory) as T;

        /// <summary>
        /// Convert the given raw input to the given model.
        /// </summary>
        public static object ConvertInputModel(Type type, Dictionary<string, string> rawInput, TKStringConverter stringConverter = null,
            Dictionary<string, string> placeholders = null, Func<string, TKCustomPropertyAttribute, Func<string, string>> placeholderTransformerFactory = null)
            => ConvertInputModel(type, 
                (rawInput == null) ? null : (propName) => rawInput?.FirstOrDefault(x => x.Key == propName).Value,
                stringConverter, placeholders, placeholderTransformerFactory);

        /// <summary>
        /// Convert the given raw input to the given model.
        /// </summary>
        public static object ConvertInputModel(Type type, Func<string, string> rawInputGetter, TKStringConverter stringConverter = null,
            Dictionary<string, string> placeholders = null, Func<string, TKCustomPropertyAttribute, Func<string, string>> placeholderTransformerFactory = null)
        {
            stringConverter ??= DefaultStringConverter;

            var instance = Activator.CreateInstance(type);
            if (rawInputGetter != null)
            {
                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    var stringValue = rawInputGetter(prop.Name);
                    if (stringValue != null)
                    {
                        var value = stringConverter.ConvertStringTo(prop.PropertyType, stringValue);

                        if (value is string str)
                        {
                            var attr = (placeholderTransformerFactory == null) ? null : TKCustomPropertyAttribute.GetFirst(prop);
                            value = ResolvePlaceholders(str, placeholders, placeholderTransformerFactory?.Invoke(prop.Name, attr));
                        }

                        prop.SetValue(instance, value);
                    }
                    else
                    {
                        prop.SetValue(instance, null);
                    }
                }
            }

            return instance;
        }

        private static string ResolvePlaceholders(string value, Dictionary<string, string> placeholders, Func<string, string> transformer)
        {
            if (string.IsNullOrWhiteSpace(value) || placeholders?.Any() != true)
            {
                return value;
            }

            foreach (var kvp in placeholders)
            {
                var key = kvp.Key;
                var placeholderValue = kvp.Value ?? "";

                if (transformer != null)
                {
                    placeholderValue = transformer.Invoke(placeholderValue) ?? "";
                }

                value = value.Replace($"{{{key?.ToUpper()}}}", placeholderValue);
            }

            return value;
        }

        /// <summary>
        /// Attempt to convert the given input to an instance of the given type.
        /// </summary>
        public static object ConvertInput(TKValueInput input, TKStringConverter stringConverter = null)
        {
            object convertedObject = null;
            var isList = input.Type.IsGenericType && input.Type.GetGenericTypeDefinition() == typeof(List<>);
            if (input.IsJson && isList)
            {
                var deserializeResult = TestRunnerService.Serializer?.DeserializeExt(input.Value, typeof(string[]));
                var itemJsons = deserializeResult?.Data as string[] ?? new string[0];
                if (deserializeResult?.HasError == true)
                {
                    throw new TKException(deserializeResult.Error);
                }

                var itemType = input.Type.GetGenericArguments()[0];
                var list = Activator.CreateInstance(input.Type) as System.Collections.IList;
                foreach (var json in itemJsons)
                {
                    if (json == null)
                    {
                        list.Add(null);
                        continue;
                    }

                    deserializeResult = TestRunnerService.Serializer?.DeserializeExt(json, itemType);
                    var item = deserializeResult?.Data;
                    if (deserializeResult?.HasError == true)
                    {
                        throw new TKException(deserializeResult.Error);
                    }
                    list.Add(item);
                }
                convertedObject = list;
            }
            else if (input.IsJson)
            {
                var deserializeResult = TestRunnerService.Serializer?.DeserializeExt(input.Value, input.Type);
                convertedObject = deserializeResult?.Data;
                if (deserializeResult?.HasError == true)
                {
                    throw new TKException(deserializeResult.Error);
                }
            }
            else if (input.IsCustomReferenceType && !string.IsNullOrWhiteSpace(input.Value))
            {
                var factory = input.ParameterFactoryFactory();

                if (isList)
                {
                    var ids = TestRunnerService.Serializer?.Deserialize(input.Value, typeof(string[])) as string[] ?? new string[0];
                    var list = Activator.CreateInstance(input.Type) as System.Collections.IList;
                    var itemType = input.Type.GetGenericArguments()[0];
                    foreach (var id in ids)
                    {
                        var item = factory?.GetInstanceByIdFor(itemType, id);
                        list.Add(item);
                    }
                    convertedObject = list;
                }
                else
                {
                    convertedObject = factory?.GetInstanceByIdFor(input.Type, input.Value);
                }
            }
            else if (!input.IsCustomReferenceType)
            {
                convertedObject = (stringConverter ?? DefaultStringConverter).ConvertStringTo(input.Type, input.Value);
            }
            return convertedObject;
        }
    }
}
