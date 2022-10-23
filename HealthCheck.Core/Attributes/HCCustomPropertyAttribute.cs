using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Attributes
{
    /// <summary></summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class HCCustomPropertyAttribute : Attribute
    {
        /// <summary>
        /// Override display name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Set a descriptive helptext.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Hint of how to display a input.
        /// </summary>
        public HCUIHint UIHints { get; set; }

        /// <summary>
        /// Use to override the label/placeholder/name displayed for any null-value.
        /// </summary>)
        public string NullName { get; set; }

        /// <summary>
        /// If <see cref="UIHints"/> include <see cref="HCUIHint.CodeArea"/>, this can be set to 'csharp', 'json', 'xml' or 'sql' to give the editor a hint of what content is displayed.
        /// <para>Defaults to 'json'</para>
        /// </summary>
        public string CodeLanguage { get; set; } = "json";

        /// <summary>
        /// Can be used on text inputs to require the input to match the given regex pattern.
        /// <para>Use JavaScript format. If not starting with a '/' one will be prepended and '/g' will be appended.</para>
        /// <para>Example: O\-\d+ or /[abc]+/gi</para>
        /// </summary>
        public string TextPattern { get; set; }

        private static readonly HCStringConverter _stringConverter = new();

        /// <summary></summary>
        protected virtual Dictionary<string, string> GetExtraValues() => new();

        /// <summary>
        /// From backend to frontend.
        /// </summary>
        public static List<HCBackendInputConfig> CreateInputConfigs(Type type,
            Action<HCBackendInputConfig, PropertyInfo, HCCustomPropertyAttribute> modifier = null)
        {
            if (type == null)
            {
                return new List<HCBackendInputConfig>();
            }

            var instance = HCReflectionUtils.TryActivate(type);
            return type.GetProperties()
                .Select(x => CreateInputConfig(x, instance, modifier))
                .ToList();
        }

        /// <summary>
        /// From backend to frontend.
        /// </summary>
        public static HCBackendInputConfig CreateInputConfig(PropertyInfo property, object instanceForDefaults = null,
            Action<HCBackendInputConfig, PropertyInfo, HCCustomPropertyAttribute> modifier = null)
        {
            var defaultValue = "";
            if (instanceForDefaults != null)
            {
                try
                {
                    defaultValue = _stringConverter.ConvertToString(property.GetValue(instanceForDefaults));
                }
                catch (Exception) { /* ignored */ }
            }

            var attr = GetFirst(property);
            var type = property.PropertyType;
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                type = Nullable.GetUnderlyingType(type);
            }

            var stringConverter = new HCStringConverter();
            var possibleValues = GetPossibleValues(type)?.Select(x => stringConverter.ConvertToString(x))?.ToList();

            var config = new HCBackendInputConfig
            {
                Id = property.Name,
                Name = attr?.Name ?? property.Name.SpacifySentence(),
                Type = CreateParameterTypeName(type),
                Description = attr?.Description ?? "",
                Nullable = isNullable,
                NullName = attr?.NullName,
                TextPattern = HCBackendInputConfig.EnsureJsRegexIsWrappedIfNotEmpty(attr?.TextPattern),
                CodeLanguage = attr?.CodeLanguage,
                DefaultValue = defaultValue,
                Flags = new(),
                UIHints = EnumUtils.GetFlaggedEnumValues<HCUIHint>(attr?.UIHints) ?? new(),
                ParameterIndex = null,
                PossibleValues = possibleValues,
                ExtraValues = attr?.GetExtraValues() ?? new Dictionary<string, string>(),
                PropertyInfo = property
            };
            modifier?.Invoke(config, property, GetFirst(property));
            return config;
        }

        /// <summary>
        /// Get possible values for the given type.
        /// </summary>
        public static List<object> GetPossibleValues(Type parameterType)
        {
            Type enumType = null;
            if (parameterType.IsEnum)
            {
                enumType = parameterType;
            }
            else if(parameterType.IsNullable() && parameterType.GenericTypeArguments[0].IsEnum)
            {
                enumType = parameterType.GenericTypeArguments[0];
            }

            // Enums
            if (enumType != null)
            {
                var isFlags = EnumUtils.IsTypeEnumFlag(enumType);
                var list = new List<object>();
                foreach (var value in Enum.GetValues(enumType))
                {
                    if (isFlags && (int)value == 0) continue;
                    list.Add(value);
                }
                return list;
            }
            // List of enums
            else if (parameterType.IsGenericType
                && parameterType.GetGenericTypeDefinition() == typeof(List<>)
                && parameterType.GetGenericArguments()[0].IsEnum)
            {
                return GetPossibleValues(parameterType.GetGenericArguments()[0]);
            }
            // Not supported
            else
            {
                return new List<object>();
            }
        }

        private static readonly Dictionary<string, string> _inputTypeAliases = new()
        {
            { "IFormFile", "HttpPostedFileBase" },
            { "Byte[]", "HttpPostedFileBase" }
        };
        /// <summary>
        /// Create parameter type name for the given type.
        /// </summary>
        public static string CreateParameterTypeName(Type type)
        {
            var typeName = type.GetFriendlyTypeName(_inputTypeAliases);
            if (type.IsEnum)
            {
                typeName = EnumUtils.IsTypeEnumFlag(type) ? "FlaggedEnum" : "Enum";
            }
            else if (type.IsNullable() && type.GenericTypeArguments[0].IsEnum)
            {
                var enumType = type.GenericTypeArguments[0];
                typeName = EnumUtils.IsTypeEnumFlag(enumType) ? "Nullable<FlaggedEnum>" : "Nullable<Enum>";
            }
            else if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(List<>)
                && type.GetGenericArguments()[0].IsEnum)
            {
                var innerType = EnumUtils.IsTypeEnumFlag(type.GetGenericArguments()[0]) ? "FlaggedEnum" : "Enum";
                typeName = $"List<{innerType}>";
            }
            else if (type.IsArray && type.GetElementType().IsNullable())
            {
                var innerType = CreateParameterTypeName(type.GetElementType());
                typeName = $"{innerType}[]";
            }
            return typeName;
        }

        /// <summary>
        /// Convert frontend input string to object prepared for conversion.
        /// </summary>
        public HCValueInput CreateValueInput(Type type, string value)
        {
            return new HCValueInput
            {
                Type = type,
                Value = value
            };
        }

        /// <summary>
        /// Get the first attribute on the given property if any.
        /// </summary>
        public static HCCustomPropertyAttribute GetFirst(PropertyInfo property)
            => property?.GetCustomAttributes(typeof(HCCustomPropertyAttribute), true)?.FirstOrDefault() as HCCustomPropertyAttribute;
    }
}
