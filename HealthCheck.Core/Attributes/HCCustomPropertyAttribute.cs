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

        private static readonly StringConverter _stringConverter = new();

        /// <summary>
        /// Create flags options for frontend consumption.
        /// </summary>
        protected virtual List<string> CreateFlags()
        {
            var flags = new List<string>();

            if (UIHints.HasFlag(HCUIHint.ReadOnlyList))
            {
                flags.Add("ReadOnlyList");
            }
            if (UIHints.HasFlag(HCUIHint.TextArea))
            {
                flags.Add("TextArea");
            }
            //if (UIHints.HasFlag(HCUIHint.CodeArea))
            //{
            //    flags.Add("CodeArea");
            //}

            return flags;
        }

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

            var instance = ReflectionUtils.TryActivate(type);
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
            var possibleValues = new List<string>();
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

            var config = new HCBackendInputConfig
            {
                Id = property.Name,
                Name = attr?.Name ?? property.Name.SpacifySentence(),
                Type = type.Name,
                Description = attr?.Description ?? "",
                Nullable = isNullable,
                NotNull = attr?.UIHints.HasFlag(HCUIHint.NotNull) == true,
                FullWidth = attr?.UIHints.HasFlag(HCUIHint.FullWidth) == true,
                DefaultValue = defaultValue,
                Flags = attr?.CreateFlags() ?? new List<string>(),
                ParameterIndex = null,
                PossibleValues = possibleValues,
                ExtraValues = attr?.GetExtraValues() ?? new Dictionary<string, string>(),
                PropertyInfo = property
            };
            modifier?.Invoke(config, property, GetFirst(property));
            return config;
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
