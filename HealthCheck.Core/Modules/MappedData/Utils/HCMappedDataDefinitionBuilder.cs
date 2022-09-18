using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.MappedData.Attributes;
using HealthCheck.Core.Modules.MappedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.MappedData.Utils
{
    /// <summary>
    /// Scans assemblies and creates definitions from classes decorated with <see cref="HCMappedClassAttribute"/>.
    /// </summary>
    public static class HCMappedDataDefinitionBuilder
	{
		/// <summary>
		/// Attempt to discover the used name from e.g. json attributes.
		/// </summary>
		public static string TryAutoDiscoverPropertyDisplayName(PropertyInfo prop, HCMappedDefinitionDiscoveryOptions options)
		{
			string autoDiscoveredName = null;
			if (prop == null) return autoDiscoveredName;
			if (options?.AllowAttributeDisplayNameResolve?.Invoke(prop) != false)
			{
				var attributes = prop.GetCustomAttributes(true);
				foreach (var attr in attributes)
				{
					var propertyNameProp = attr?.GetType()?.GetProperty("PropertyName") ?? attr?.GetType()?.GetProperty("Name");
					autoDiscoveredName = propertyNameProp?.GetValue(attr) as string;
					if (!string.IsNullOrWhiteSpace(autoDiscoveredName)) break;
				}
			}
			return autoDiscoveredName ?? prop.Name;
		}

		/// <summary>
		/// Scans assemblies and creates definitions from classes decorated with <see cref="HCMappedClassAttribute"/>.
		/// <para>Key = class type, Value = definition.</para>
		/// </summary>
		public static HCMappedDataDefinitions CreateDefinitions(IEnumerable<Assembly> assemblies, HCMappedDefinitionDiscoveryOptions options)
		{
			var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToArray();

			var refTypes = allTypes
				.Select(x => (type: x, attr: x.GetCustomAttribute<HCMappedReferencedTypeAttribute>()))
				.Where(x => x.attr != null);
			var refDefs = new List<HCMappedReferencedTypeDefinition>();
			foreach (var (type, attr) in refTypes)
			{
				var def = CreateRefTypeDefinition(type, attr);
				refDefs.Add(def);
			}

			var rootTypes = allTypes
				.Select(x => (type: x, attr: x.GetCustomAttributes<HCMappedClassAttribute>().ToArray()))
				.Where(x => x.attr != null);
			var classDefs = new List<HCMappedClassDefinition>();
			foreach (var rootType in rootTypes)
			{
				var index = 0;
				foreach (var attr in rootType.attr)
				{
					var def = CreateClassDefinition(rootType.type, attr, options, refDefs, index: index);
					classDefs.Add(def);
					index++;
				}
			}

			return new HCMappedDataDefinitions
			{
				ClassDefinitions = classDefs.OrderBy(x => x.Id).ToList(),
				ReferencedDefinitions = refDefs.OrderBy(x => x.Id).ToList()
			};
		}

		private static HCMappedReferencedTypeDefinition CreateRefTypeDefinition(Type type, HCMappedReferencedTypeAttribute attribute)
		{
			return new HCMappedReferencedTypeDefinition
			{
				Id = CreateMappedClassTypeId(type, null),
				DisplayName = attribute?.DisplayName ?? type.Name,
				NameInMapping = attribute?.NameInMapping ?? type.Name,
				Attribute = attribute,
				ReferenceId = attribute?.ReferenceId ?? type.Name,
				Type = type
			};
		}

		private static HCMappedClassDefinition CreateClassDefinition(Type type, HCMappedClassAttribute attribute, HCMappedDefinitionDiscoveryOptions options,
			List<HCMappedReferencedTypeDefinition> refDefs,
			HCMappedClassDefinition parent = null, int index = 0)
        {
            attribute ??= type.GetCustomAttribute<HCMappedClassAttribute>();

            string mappingSrc = ExtractMappingSrc(type, attribute);
            var mapping = HCMappedDataMappingParser.ParseMapping(type, mappingSrc, refDefs);

            var memberDefinitions = new List<HCMappedMemberDefinition>();
            foreach (var member in type.GetProperties())
            {
                var memberMapping = mapping.Objects.FirstOrDefault(x => x.PropertyInfo == member);
                if (memberMapping == null) continue;

                var memberDef = CreateMemberDefinition(memberMapping, options, null);
                if (memberDef != null) memberDefinitions.Add(memberDef);
            }

            var mappingsWithoutProperties = mapping.Objects.Where(x => !x.IsValid && !memberDefinitions.Any(m => m.PropertyName == x.Name));
            foreach (var mappingMember in mappingsWithoutProperties)
            {
                var memberDef = CreateMemberDefinition(mappingMember, options, null);
                if (memberDef != null) memberDefinitions.Add(memberDef);
            }

            return new HCMappedClassDefinition
            {
                Id = CreateMappedClassTypeId(type, parent, index),
                ClassType = type,
                GroupName = attribute?.GroupName ?? parent?.GroupName,
                Attribute = attribute,
                MemberDefinitions = memberDefinitions,
                TypeName = type.Name,
                DisplayName = attribute?.OverrideName ?? type.Name
            };
        }

        private static string ExtractMappingSrc(Type type, HCMappedClassAttribute attribute)
        {
            var mappingSrc = attribute.Mapping;
            if (string.IsNullOrWhiteSpace(mappingSrc) && !string.IsNullOrWhiteSpace(attribute.MappingFromMethodName))
            {
                var methodName = attribute.MappingFromMethodName;
                if (!methodName.Contains("."))
                {
                    var method = type.GetMethod(methodName);
                    if (method == null) throw new HCException($"Mapping method '{methodName}' was not found in type '{type.Name}'.");
                    mappingSrc = method.Invoke(null, new object[0]) as string;
                }
                else
                {
                    var className = methodName.Substring(0, methodName.IndexOf('.'));
                    var classType = type.GetNestedType(className);
                    if (classType == null) throw new HCException($"Could not find class type '{className}' within '{classType.Name}'.");
                    var classInstance = Activator.CreateInstance(classType);

                    methodName = methodName.Substring(className.Length + 1);
                    var method = classType.GetMethod(methodName);
                    if (method == null) throw new HCException($"Mapping method '{methodName}' was not found in type '{classType.Name}'.");
                    mappingSrc = method.Invoke(classInstance, new object[0]) as string;
                }
            }

            return mappingSrc;
        }

        private static string CreateMappedClassTypeId(Type type, HCMappedClassDefinition parent, int index = 0)
		{
			var suffix = (index == 0) ? string.Empty : $"_{index}";
			if (type == null) return null;
			else if (parent == null) return $"{type?.Namespace}.{type?.Name}{suffix}";
			else return $"{parent.Id}.{type?.Name}{suffix}";
		}

		private static HCMappedMemberDefinition CreateMemberDefinition(HCMappedDataMappingParser.ParsedMappingObject member, HCMappedDefinitionDiscoveryOptions options, HCMappedMemberDefinition parent)
		{
			var prop = member.PropertyInfo;
			var displayName = options?.MemberDisplayNameOverride?.Invoke(prop) ?? TryAutoDiscoverPropertyDisplayName(prop, options) ?? member.Name;

			var def = new HCMappedMemberDefinition
			{
				Id = prop?.Name ?? member.Name,
				Member = prop,
				PropertyName = prop?.Name ?? "[not found]",
				FullPropertyPath = $"{parent?.FullPropertyPath}.{(prop?.Name ?? member.Name)}".TrimStart('.'),
				DisplayName = displayName,
				Parent = parent,
				Remarks = member.Comment,
				IsValid = member.IsValid,
				Error = member.Error
			};

			var children = new List<HCMappedMemberDefinition>();
			foreach (var childMember in member.Children)
			{
				children.Add(CreateMemberDefinition(childMember, options, def));
			}
			def.Children = children;

			var mappedTo = new List<HCMappedMemberReferenceDefinition>();
			foreach (var item in member.MappedTo)
			{
				var pathItems = item.Chain.Items.Select(x => new HCMappedMemberReferencePathItemDefinition
				{
					Success = x.Success,
					Error = x.Error,
					DisplayName = (x.PropertyInfo == null) ? x.Name : TryAutoDiscoverPropertyDisplayName(x.PropertyInfo, options),
					PropertyName = x.PropertyInfo?.Name,
					PropertyType = x.PropertyInfo?.PropertyType,
					PropertyInfo = x.PropertyInfo,
					DeclaringType = x.DeclaringType
				}).ToList();
				mappedTo.Add(new HCMappedMemberReferenceDefinition
				{
					Items = pathItems,
					Path = item.Chain.Path,
					Success = item.Chain.Success,
					Error = item.Chain.Error,
					RootType = item.Chain.RootType,
					RootReferenceId = item.DottedPath.Split('.')[0].Trim()
				});
			}
			def.MappedTo = mappedTo;

			return def;
		}
	}
}
