using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Conversion utils.
    /// </summary>
    public static class HCValueConversionUtils
    {
        internal static StringConverter DefaultStringConverter { get; set; } = new StringConverter();

        /// <summary>
        /// Convert the given raw input to the given model-
        /// </summary>
        public static T ConvertInputModel<T>(Dictionary<string, string> rawInput, StringConverter stringConverter = null)
            where T: class, new()
            => ConvertInputModel(typeof(T), rawInput, stringConverter) as T;

        /// <summary>
        /// Convert the given raw input to the given model-
        /// </summary>
        public static T ConvertInputModel<T>(Func<string, string> rawInputGetter, StringConverter stringConverter = null)
            where T : class, new()
            => ConvertInputModel(typeof(T), rawInputGetter, stringConverter) as T;

        /// <summary>
        /// Convert the given raw input to the given model-
        /// </summary>
        public static object ConvertInputModel(Type type, Dictionary<string, string> rawInput, StringConverter stringConverter = null)
            => ConvertInputModel(type, 
                (rawInput == null) ? null : (propName) => rawInput?.FirstOrDefault(x => x.Key == propName).Value,
                stringConverter);

        /// <summary>
        /// Convert the given raw input to the given model-
        /// </summary>
        public static object ConvertInputModel(Type type, Func<string, string> rawInputGetter, StringConverter stringConverter = null)
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
                        prop.SetValue(instance, value);
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Attempt to convert the given input to an instance of the given type.
        /// </summary>
        public static object ConvertInput(HCValueInput input, StringConverter stringConverter = null)
        {
            object convertedObject = null;
            if (input.IsJson)
            {
                convertedObject = TestRunnerService.Serializer?.Deserialize(input.Value, input.Type);
                var error = TestRunnerService.Serializer?.LastError;
                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new HCException(error);
                }
            }
            else if (input.IsCustomReferenceType && !string.IsNullOrWhiteSpace(input.Value))
            {
                var factory = input.ParameterFactoryFactory();
                convertedObject = factory?.GetInstanceByIdFor(input.Type, input.Value);
            }
            else if (!input.IsCustomReferenceType)
            {
                convertedObject = (stringConverter ?? DefaultStringConverter).ConvertStringTo(input.Type, input.Value);
            }
            return convertedObject;
        }
    }
}
