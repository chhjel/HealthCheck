using HealthCheck.Core.Extensions;
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
        /// Disallow null values, even for strings.
        /// </summary>
        public bool PreventNulls { get; set; }

        // todo: uihints enum: TextArea, ReadOnlyList

        private static StringConverter _stringConverter = new StringConverter();

        /// <summary>
        /// From backend to frontend.
        /// </summary>
        public static List<HCBackendInputConfig> CreateInputConfigs(Type type)
        {
            var instance = ReflectionUtils.TryActivate(type);
            return type.GetProperties()
                .Select(x => CreateInputConfig(x, instance))
                .ToList();
        }

        /// <summary>
        /// From backend to frontend.
        /// </summary>
        public static HCBackendInputConfig CreateInputConfig(PropertyInfo property, object instanceForDefaults = null)
        {
            var flags = new List<string>();
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

            return new HCBackendInputConfig
            {
                Id = property.Name,
                Name = attr?.Name ?? property.Name.SpacifySentence(),
                Type = type.Name,
                Description = attr?.Description ?? "",
                Nullable = isNullable,
                NotNull = attr?.PreventNulls == true,
                DefaultValue = defaultValue,
                Flags = flags,
                ParameterIndex = null,
                PossibleValues = possibleValues
            };
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
