using QoDL.Toolkit.Core.Exceptions;
using QoDL.Toolkit.Core.Modules.MappedData.Attributes;
using QoDL.Toolkit.Core.Modules.MappedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.MappedData.Utils
{
    /// <summary>
    /// Scans assemblies and creates definitions from classes decorated with <see cref="TKMappedClassAttribute"/>.
    /// </summary>
    public static class TKMappedDataDefinitionBuilder
	{
		/// <summary>
		/// Attempt to discover the used name from e.g. json attributes.
		/// </summary>
		public static string TryAutoDiscoverPropertyDisplayName(PropertyInfo prop, TKMappedDefinitionDiscoveryOptions options)
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
		/// Scans assemblies and creates definitions from classes decorated with <see cref="TKMappedClassAttribute"/>.
		/// <para>Key = class type, Value = definition.</para>
		/// </summary>
		public static TKMappedDataDefinitions CreateDefinitions(IEnumerable<Assembly> assemblies, TKMappedDefinitionDiscoveryOptions options)
		{
			var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToArray();

			var refTypes = allTypes
				.Select(x => (type: x, attr: x.GetCustomAttribute<TKMappedReferencedTypeAttribute>()))
				.Where(x => x.attr != null);
			var refDefs = new List<TKMappedReferencedTypeDefinition>();
			foreach (var (type, attr) in refTypes)
			{
				var def = CreateRefTypeDefinition(type, attr);
				refDefs.Add(def);
			}

			var rootTypes = allTypes
				.Select(x => (type: x, attr: x.GetCustomAttributes<TKMappedClassAttribute>().ToArray()))
				.Where(x => x.attr != null);
			var classDefs = new List<TKMappedClassDefinition>();
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

			return new TKMappedDataDefinitions
			{
				ClassDefinitions = classDefs.OrderBy(x => x.Id).ToList(),
				ReferencedDefinitions = refDefs.OrderBy(x => x.Id).ToList()
			};
		}

		private static TKMappedReferencedTypeDefinition CreateRefTypeDefinition(Type type, TKMappedReferencedTypeAttribute attribute)
		{
			return new TKMappedReferencedTypeDefinition
			{
				Id = CreateMappedClassTypeId(type, null),
				TypeName = type.Name,
				DisplayName = attribute?.DisplayName ?? type.Name,
				NameInMapping = attribute?.NameInMapping ?? type.Name,
				Attribute = attribute,
				ReferenceId = attribute?.ReferenceId ?? type.Name,
				Type = type
			};
		}

		private static TKMappedClassDefinition CreateClassDefinition(Type type, TKMappedClassAttribute attribute, TKMappedDefinitionDiscoveryOptions options,
			List<TKMappedReferencedTypeDefinition> refDefs,
			TKMappedClassDefinition parent = null, int index = 0)
        {
            attribute ??= type.GetCustomAttribute<TKMappedClassAttribute>();

            string mappingSrc = ExtractMappingSrc(type, attribute);
            var mapping = TKMappedDataMappingParser.ParseMapping(type, mappingSrc, refDefs);

			var memberDefinitions = new List<TKMappedMemberDefinition>();
			var allMemberDefinitions = new List<TKMappedMemberDefinition>();
			foreach (var member in type.GetProperties())
            {
                var memberMapping = mapping.Objects.FirstOrDefault(x => x.PropertyInfo == member);
                if (memberMapping == null) continue;

                var memberDef = CreateMemberDefinition(memberMapping, options, null, (d) => allMemberDefinitions.Add(d));
				if (memberDef != null)
				{
					memberDefinitions.Add(memberDef);
					allMemberDefinitions.Add(memberDef);
				}
            }

            var mappingsWithoutProperties = mapping.Objects.Where(x => !x.IsValid && !memberDefinitions.Any(m => m.PropertyName == x.Name));
            foreach (var mappingMember in mappingsWithoutProperties)
            {
                var memberDef = CreateMemberDefinition(mappingMember, options, null, (d) => allMemberDefinitions.Add(d));
				if (memberDef != null)
				{
					memberDefinitions.Add(memberDef);
					allMemberDefinitions.Add(memberDef);
				}
            }

            return new TKMappedClassDefinition
            {
                Id = CreateMappedClassTypeId(type, parent, index),
                ClassType = type,
                GroupName = attribute?.GroupName ?? parent?.GroupName,
                Attribute = attribute,
                MemberDefinitions = memberDefinitions,
				AllMemberDefinitions = allMemberDefinitions,
                TypeName = type.Name,
                DisplayName = attribute?.OverrideName ?? type.Name
            };
        }

        private static string ExtractMappingSrc(Type type, TKMappedClassAttribute attribute)
        {
            var mappingSrc = attribute.Mapping;
            if (string.IsNullOrWhiteSpace(mappingSrc) && !string.IsNullOrWhiteSpace(attribute.MappingFromMethodName))
            {
                var methodName = attribute.MappingFromMethodName;
                if (!methodName.Contains("."))
                {
                    var method = type.GetMethod(methodName);
                    if (method == null) throw new TKException($"Mapping method '{methodName}' was not found in type '{type.Name}'.");
                    mappingSrc = method.Invoke(null, new object[0]) as string;
                }
                else
                {
                    var className = methodName.Substring(0, methodName.IndexOf('.'));
                    var classType = type.GetNestedType(className);
                    if (classType == null) throw new TKException($"Could not find class type '{className}' within '{classType.Name}'.");
                    var classInstance = Activator.CreateInstance(classType);

                    methodName = methodName.Substring(className.Length + 1);
                    var method = classType.GetMethod(methodName);
                    if (method == null) throw new TKException($"Mapping method '{methodName}' was not found in type '{classType.Name}'.");
                    mappingSrc = method.Invoke(classInstance, new object[0]) as string;
                }
            }

            return mappingSrc;
        }

        private static string CreateMappedClassTypeId(Type type, TKMappedClassDefinition parent, int index = 0)
		{
			var suffix = (index == 0) ? string.Empty : $"_{index}";
			if (type == null) return null;
			else if (parent == null) return $"{type?.Namespace}.{type?.Name}{suffix}";
			else return $"{parent.Id}.{type?.Name}{suffix}";
		}

		private static TKMappedMemberDefinition CreateMemberDefinition(TKMappedDataMappingParser.ParsedMappingObject member, TKMappedDefinitionDiscoveryOptions options, TKMappedMemberDefinition parent,
			Action<TKMappedMemberDefinition> onChildCreated)
		{
			var prop = member.PropertyInfo;
			var displayName = options?.MemberDisplayNameOverride?.Invoke(prop) ?? TryAutoDiscoverPropertyDisplayName(prop, options) ?? member.Name;

			var def = new TKMappedMemberDefinition
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

			var children = new List<TKMappedMemberDefinition>();
			foreach (var childMember in member.Children)
			{
				var child = CreateMemberDefinition(childMember, options, def, onChildCreated);
				children.Add(child);
				onChildCreated(child);
			}
			def.Children = children;

			var mappedTo = new List<TKMappedMemberReferenceDefinition>();
			foreach (var item in member.MappedTo)
			{
				List<TKMappedMemberReferencePathItemDefinition> items = null;
				if (item.IsHardCoded)
				{
					items = new List<TKMappedMemberReferencePathItemDefinition> {
						new TKMappedMemberReferencePathItemDefinition { HardCodedValue = item.HardCodedValue.TrimStart('"').TrimEnd('"') }
					};
                }
				else
                {
					items = item.Chain.Items.Select(x =>
					{
						var indexer = string.Empty;
						if (x.Name?.EndsWith("]") == true)
						{
							indexer = x.Name.Substring(x.Name.LastIndexOf("["));
						}
						var mappedToDisplayName = (x.PropertyInfo == null)
							? x.Name
							: (TryAutoDiscoverPropertyDisplayName(x.PropertyInfo, options) + indexer);
						return new TKMappedMemberReferencePathItemDefinition
						{
							Success = x.Success,
							Error = x.Error,
							DisplayName = mappedToDisplayName,
							PropertyName = x.PropertyInfo?.Name,
							PropertyType = x.PropertyInfo?.PropertyType,
							PropertyInfo = x.PropertyInfo,
							DeclaringType = x.DeclaringType
						};
					}).ToList();
				}

				mappedTo.Add(new TKMappedMemberReferenceDefinition
				{
					Items = items,
					Path = item.Chain?.Path,
					Success = item.IsHardCoded || item.Chain?.Success == true,
					Error = item.Chain?.Error,
					RootType = item.Chain?.RootType,
					RootReferenceId = item.DottedPath?.Split('.')[0].Trim()
				});
			}
			def.MappedTo = mappedTo;

			return def;
		}
	}
}
