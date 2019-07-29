using HealthCheck.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Converts strings to other types.
    /// </summary>
    public class StringConverter
    {
        private class ConverterPair
        {
            public Func<string, object> StringToObjConverter { get; set; }
            public Func<object, string> ObjToStringConverter { get; set; }
        }
        private Dictionary<Type, ConverterPair> ConversionHandlers { get; set; } = new Dictionary<Type, ConverterPair>();

        private readonly string[] BOOL_ALIAS_TRUE = new[] { "yes", "ja", "jup" };
        private readonly string[] BOOL_ALIAS_FALSE = new[] { "no", "nei", "nope", "0x90" };
        private readonly string[] DATETIME_ALIAS_NOW = new[] { "now", "today" };

        /// <summary>
        /// Register a new converter that converts a string to the given type.
        /// </summary>
        public void RegisterConverter<T>(Func<string, object> stringToObjConverter, Func<object, string> objToStringConverter)
        {
            ConversionHandlers[typeof(T)] = new ConverterPair()
            {
                ObjToStringConverter = objToStringConverter,
                StringToObjConverter = stringToObjConverter
            };
        }

        /// <summary>
        /// Attempt to convert the given object into a string.
        /// </summary>
        public string ConvertToString(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var objType = obj.GetType();

            // Use registered handler if any.
            if (ConversionHandlers.ContainsKey(objType) && ConversionHandlers[objType].ObjToStringConverter != null)
            {
                return ConversionHandlers[objType].ObjToStringConverter(obj);
            }
            // Serialize lists as json
            else if(objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
            {
                using (var memStream = new MemoryStream())
                {
                    var ser = new DataContractJsonSerializer(objType);
                    ser.WriteObject(memStream, obj);

                    memStream.Position = 0;
                    using (var reader = new StreamReader(memStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            // Fallback to basic stringifying.
            return obj?.ToString();
        }

        /// <summary>
        /// Attempt to convert the given string to the given type.
        /// </summary>
        public object ConvertStringTo(Type type, string input)
        {
            var method = GetType().GetMethods()
                .Where(x => x.Name == nameof(ConvertStringTo) && x.IsGenericMethod)
                .First();

            // Handle nullable types
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                if (input == null)
                {
                    return null;
                }

                var valueType = type.GetGenericArguments()[0];
                var convertedValue = ConvertStringTo(valueType, input);
                var nullableType = typeof(Nullable<>).MakeGenericType(valueType);
                var resultNullable = Activator.CreateInstance(nullableType, convertedValue);
                return resultNullable;
            }

            var genericMethod = method.MakeGenericMethod(type);
            var value = genericMethod.Invoke(this, new object[] { input });
            return Convert.ChangeType(value, type);
        }

        /// <summary>
        /// Attempt to convert the given string to the given type.
        /// </summary>
        public T ConvertStringTo<T>(string input)
        {
            var inputType = typeof(T);
            if (input == null && inputType != typeof(string))
            {
                input = input ?? "";
            }

            // Use registered handler if any.
            if (ConversionHandlers.ContainsKey(inputType) && ConversionHandlers[inputType].StringToObjConverter != null)
            {
                return (T)Convert.ChangeType(ConversionHandlers[inputType].StringToObjConverter(input), typeof(T));
            }

            // Fallback to default built in logic
            var trimmedLowerInput = input?.Trim()?.ToLower() ?? "";

            // Special cases
            if (inputType == typeof(Boolean) && BOOL_ALIAS_TRUE.Contains(trimmedLowerInput))
                return (T)Convert.ChangeType(true, typeof(T));
            else if (inputType == typeof(Boolean) && BOOL_ALIAS_FALSE.Contains(trimmedLowerInput))
                return (T)Convert.ChangeType(false, typeof(T));
            else if (inputType == typeof(DateTime) && DATETIME_ALIAS_NOW.Contains(trimmedLowerInput))
                return (T)Convert.ChangeType(DateTime.Now, typeof(T));

            // Fallback to TryParse or throw
            if (inputType == typeof(String))
                return (T)Convert.ChangeType(input, typeof(T));
            else if (inputType == typeof(Boolean))
                return ParseOrThrow<T>((i) => Boolean.Parse(i), input);
            else if (inputType == typeof(Byte))
                return ParseOrThrow<T>((i) => Byte.Parse(i), input);
            else if (inputType == typeof(Char))
                return ParseOrThrow<T>((i) => Char.Parse(i), input);
            else if (inputType == typeof(DateTime))
                return ParseOrThrow<T>((i) => DateTime.Parse(i), input);
            else if (inputType == typeof(Decimal))
                return ParseOrThrow<T>((i) => Decimal.Parse(i), input);
            else if (inputType == typeof(Double))
                return ParseOrThrow<T>((i) => Double.Parse(i), input);
            else if (inputType == typeof(SByte))
                return ParseOrThrow<T>((i) => SByte.Parse(i), input);
            else if (inputType == typeof(Single))
                return ParseOrThrow<T>((i) => Single.Parse(i), input);
            else if (inputType == typeof(UInt16))
                return ParseOrThrow<T>((i) => UInt16.Parse(i), input);
            else if (inputType == typeof(UInt32))
                return ParseOrThrow<T>((i) => UInt32.Parse(i), input);
            else if (inputType == typeof(UInt64))
                return ParseOrThrow<T>((i) => UInt64.Parse(i), input);
            else if (inputType == typeof(Int16))
                return ParseOrThrow<T>((i) => Int16.Parse(i), input);
            else if (inputType == typeof(Int32))
                return ParseOrThrow<T>((i) => Int32.Parse(i), input);
            else if (inputType == typeof(Int64))
                return ParseOrThrow<T>((i) => Int64.Parse(i), input);
            else
                throw new ConversionHandlerNotFoundException(input, typeof(T));
        }

        private T ParseOrThrow<T>(Func<string, object> parser, string input)
        {
            try
            {
                var type = typeof(T);
                var parsedValue = parser(input);
                return (T)Convert.ChangeType(parsedValue, type);
            }
            catch (System.Exception)
            {
                throw new ConversionFailedException(input, typeof(T));
            }
        }
    }
}
