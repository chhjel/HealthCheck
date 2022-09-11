using HealthCheck.Core.Modules.MappedData.Attributes;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary>
    /// A definitions of a class being mapped to another class.
    /// </summary>
    public class HCMappedClassDefinition
	{
		/// <summary>
		/// Internally used id of this definition.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Name of the class type.
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Display name of this mapping.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Id of other definition this one maps to.
		/// </summary>
		public string MapsToDefinitionId { get; set; }

		/// <summary>
		/// Group name if specified.
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Type of the other class this definition maps to.
		/// </summary>
		public Type MapsToType { get; set; }

		/// <summary>
		/// Definition this one maps to.
		/// </summary>
		public HCMappedClassDefinition MapsToDefinition { get; set; }

		/// <summary>
		/// Type of the class this definition belongs to.
		/// </summary>
		public Type ClassType { get; set; }

		/// <summary>
		/// Attribute decorated on the definition class type.
		/// </summary>
		public HCMappedClassAttribute Attribute { get; set; }

		/// <summary>
		/// Member definitions from any properties.
		/// </summary>
		public List<HCMappedMemberDefinition> MemberDefinitions { get; set; }

        /*
		public class HCAutoMappingOptions
		{
			public Func<HCMappedMemberDefinition, bool> MemberFilter { get; set; }
			public Dictionary<string, Func<object, object>> ConvertersBySourcePropertyName { get; set; } = new();
			public Dictionary<Type, Func<Type, object, object>> ConvertersBySourceType { get; set; } = new();
		}
		public TTarget MapToNewInstance<TSource, TTarget>(TSource source, HCAutoMappingOptions options = null) where TTarget: new()
		{
			var target = new TTarget();
			MapToInstance<TSource, TTarget>(source, target, options);
			return target;
		}

		public void MapToInstance<TSource, TTarget>(TSource source, TTarget target, HCAutoMappingOptions options = null)
		{
			// todo cache mapper in dict<TSourceTTarget, action>
			var sourceType = typeof(TSource);
			var targetType = typeof(TTarget);
			foreach (var member in MemberDefinitions)
			{
				if (options?.MemberFilter?.Invoke(member) == false) continue;

				var from = sourceType.GetProperty(member.CodeName);
				var to = targetType.GetProperty(member.MappedTo);

				var srcValue = from.GetValue(source);

				if (options?.ConvertersBySourcePropertyName != null && options.ConvertersBySourcePropertyName.TryGetValue(from.Name, out var customConverterByName) == true)
				{
					srcValue = customConverterByName(srcValue);
				}
				else if (options?.ConvertersBySourceType != null && options.ConvertersBySourceType.TryGetValue(from.PropertyType, out var customConverterByType) == true)
				{
					srcValue = customConverterByType(to.PropertyType, srcValue);
				}
				else if (srcValue != null && srcValue.GetType() != to.PropertyType)
				{
					srcValue = Convert.ChangeType(srcValue, to.PropertyType);
				}

				to.SetValue(target, srcValue);
			}
		}
		*/
    }

}
