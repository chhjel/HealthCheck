using QoDL.Toolkit.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Converts strings to other types.
/// </summary>
public class TKStringConverter
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
    private static readonly CultureInfo _culture = new("en-US");

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
        // Special case for enums lists for now
        else if (objType.IsGenericType
           && objType.GetGenericTypeDefinition() == typeof(List<>)
           && objType.GetGenericArguments()[0].IsEnum)
        {
            return SerializeCollection(obj, objType.GetGenericArguments()[0]);
        }
        // Special case for enums arrays for now
        else if (objType.IsArray && objType.GetElementType().IsEnum)
        {
            return SerializeCollection(obj, objType.GetElementType());
        }
        // Serialize lists as json
        else if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return JsonSerialize(obj);
        }
        // Handle culture specific
        else if (obj is DateTime date) return date.ToString(_culture);
        else if (obj is DateTimeOffset dateOffset) return dateOffset.ToString(_culture);
        else if (obj is float floatValue) return floatValue.ToString(_culture);
        else if (obj is double doubleValue) return doubleValue.ToString(_culture);
        else if (obj is decimal decimalValue) return decimalValue.ToString(_culture);

        // Fallback to basic stringifying.
        return obj?.ToString();
    }


    /// <summary>
    /// Attempt to convert the given string to the given type.
    /// </summary>
    public object ConvertStringTo(Type type, string input)
    {
        var method = GetType().GetMethods()
            .First(x => x.Name == nameof(ConvertStringTo) && x.IsGenericMethod);

        // Handle nullable types
        var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        if (isNullable)
        {
            if (input == null)
            {
                return null;
            }

            var valueType = type.GetGenericArguments()[0];
            if (valueType.IsEnum && !Enum.GetValues(valueType).Cast<Enum>().Any(x => x.ToString() == input))
            {
                return null;
            }

            var convertedValue = ConvertStringTo(valueType, input);
            var nullableType = typeof(Nullable<>).MakeGenericType(valueType);
            var resultNullable = Activator.CreateInstance(nullableType, convertedValue);
            return resultNullable;
        }

        var genericMethod = method.MakeGenericMethod(type);
        var value = genericMethod.Invoke(this, new object[] { input });
        if (value != null && (value.GetType() == type || type.IsInstanceOfType(value))) return value;
        else return Convert.ChangeType(value, type);
    }

    /// <summary>
    /// Attempt to convert the given string to the given type.
    /// </summary>
    public T ConvertStringTo<T>(string input)
    {
        var inputType = typeof(T);
        if (input == null && inputType != typeof(string))
        {
            input ??= "";
        }

        if (inputType.IsEnum)
        {
            if (input == "" && TKEnumUtils.IsTypeEnumFlag(inputType))
            {
                return default;
            }
            return (T)Enum.Parse(inputType, input);
        }
        else if (inputType.IsGenericType
           && inputType.GetGenericTypeDefinition() == typeof(List<>)
           && inputType.GetGenericArguments()[0].IsEnum)
        {
            var enumType = inputType.GetGenericArguments()[0];
            input = input?.Replace("[", "")?.Replace("]", "") ?? "";
            var enumNames = input.Split(new[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Replace("\"", ""));

            var listInstance = Activator.CreateInstance(inputType);
            var listAddMethod = inputType.GetMethod("Add");
            enumNames.Select(x => Enum.Parse(enumType, x)).ToList().ForEach(x => listAddMethod.Invoke(listInstance, new object[] { x }));
            return (T)listInstance;
        }
        // Use registered handler if any.
        else if (ConversionHandlers.ContainsKey(inputType) && ConversionHandlers[inputType].StringToObjConverter != null)
        {
            var obj = ConversionHandlers[inputType].StringToObjConverter(input);
            if (
                obj is T
                || (typeof(T).IsGenericType && typeof(T).GetGenericArguments()[0].IsAssignableFrom(obj.GetType().GetGenericArguments()[0]))
            )
            {
                return (T)obj;
            }
            else return (T)Convert.ChangeType(ConversionHandlers[inputType].StringToObjConverter(input), typeof(T));
        }
        else if (inputType.IsGenericType
           && inputType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return (T)JsonDeserialize(inputType, input);
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
        else if (inputType == typeof(DateTimeOffset) && DATETIME_ALIAS_NOW.Contains(trimmedLowerInput))
            return (T)Convert.ChangeType(DateTimeOffset.Now, typeof(T));
        // Date ranges
        else if (inputType == typeof(DateTime[]) || inputType == typeof(DateTime?[])
            || inputType == typeof(DateTimeOffset[]) || inputType == typeof(DateTimeOffset?[]))
        {
            var parts = input?.Split(new[] { " - " }, 2, StringSplitOptions.None) ?? new string[0];
            string startStr = null;
            string endStr = null;
            if (parts.Length >= 2)
            {
                startStr = parts[0];
                endStr = parts[1];
            }
            static DateTime parseDate(string val) => string.IsNullOrWhiteSpace(val) ? DateTime.Now : DateTime.Parse(val);
            static DateTime? parseDateNullable(string val) => string.IsNullOrWhiteSpace(val) ? null : DateTime.Parse(val);
            static DateTimeOffset parseDateOffset(string val) => string.IsNullOrWhiteSpace(val) ? DateTimeOffset.Now : DateTimeOffset.Parse(val);
            static DateTimeOffset? parseDateOffsetNullable(string val) => string.IsNullOrWhiteSpace(val) ? null : DateTimeOffset.Parse(val);

            if (inputType == typeof(DateTime[])) return (T)Convert.ChangeType(new[] { parseDate(startStr), parseDate(endStr) }, typeof(T));
            if (inputType == typeof(DateTime?[])) return (T)Convert.ChangeType(new[] { parseDateNullable(startStr), parseDateNullable(endStr) }, typeof(T));
            if (inputType == typeof(DateTimeOffset[])) return (T)Convert.ChangeType(new[] { parseDateOffset(startStr), parseDateOffset(endStr) }, typeof(T));
            else return (T)Convert.ChangeType(new[] { parseDateOffsetNullable(startStr), parseDateOffsetNullable(endStr) }, typeof(T));
        }

        // 2022-08-07T09:45:24 - 2022-08-07T09:45:24

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
        else if (inputType == typeof(DateTimeOffset))
            return ParseOrThrow<T>((i) => DateTimeOffset.Parse(i), input);
        else if (inputType == typeof(TimeSpan))
            return ParseOrThrow<T>((i) =>
            {
                var parts = i.Split(':').Select(x => double.Parse(x)).ToArray();
                var ms = (parts[2] - (int)parts[2]) * 1000;
                return new TimeSpan(0, (int)parts[0], (int)parts[1], (int)parts[2], (int)ms);
            }, input);
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
        else if (inputType == typeof(Guid))
            return ParseOrThrow<T>((i) => string.IsNullOrWhiteSpace(i) ? Guid.Empty : Guid.Parse(i), input);
        else
            throw new TKConversionHandlerNotFoundException(input, typeof(T));
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
            throw new TKConversionFailedException(input, typeof(T));
        }
    }

    private string JsonSerialize(object obj)
    {
        var objType = obj.GetType();

        var memStream = new MemoryStream();
        var ser = new DataContractJsonSerializer(objType);
        ser.WriteObject(memStream, obj);

        memStream.Position = 0;
        using var reader = new StreamReader(memStream);
        return reader.ReadToEnd();
    }

    private object JsonDeserialize(Type type, string json)
    {
        using var memStream = new MemoryStream(Encoding.Unicode.GetBytes(json));
        var deserializer = new DataContractJsonSerializer(type);
        return deserializer.ReadObject(memStream);
    }

    private string SerializeCollection(object list, Type itemType)
    {
        var method = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(x => x.Name == nameof(SerializeCollection) && x.IsGenericMethod);

        return method.MakeGenericMethod(itemType).Invoke(this, new object[] { list }) as string;
    }
    private string SerializeCollection<T>(IList<T> list)
    {
        return $"[{string.Join(", ", list.Select(x => $"\"{x?.ToString()}\""))}]";
    }
}
