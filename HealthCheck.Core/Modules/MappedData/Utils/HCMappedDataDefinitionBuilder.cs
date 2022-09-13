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
		/// Scans assemblies and creates definitions from classes decorated with <see cref="HCMappedClassAttribute"/>.
		/// <para>Key = class type, Value = definition.</para>
		/// </summary>
		public static Dictionary<Type, HCMappedClassDefinition> CreateDefinitions(IEnumerable<Assembly> assemblies, HCMappedDefinitionDiscoveryOptions options)
		{
			// First discover all relevant classes
			var defsById = new Dictionary<string, HCMappedClassDefinition>();
			var types = assemblies.SelectMany(x => x.GetTypes());
			foreach (var type in types)
			{
				CreateDefinitionsFromType(type, defsById, options);
			}

			// Then create links
			foreach (var def in defsById)
			{
				var otherId = def.Value.MapsToDefinitionId;
				if (!string.IsNullOrWhiteSpace(otherId) && defsById.TryGetValue(otherId, out var other))
				{
					other.MapsToDefinitionId = def.Value.Id;
					other.MapsToDefinition = def.Value;
					other.MapsToType = def.Value.ClassType;

					// Mark referenced members
					var selfMembers = def.Value.MemberDefinitions;
					var otherMembers = other.MemberDefinitions;
					foreach (var selfMember in selfMembers)
					{
						// Already tagged
						if (selfMember.IsReferenced) continue;

						// Self attribute references an existing other member
						HCMappedMemberDefinition referencedOther = (selfMember.Attribute == null) ? null : otherMembers.FirstOrDefault(other => selfMember.Attribute.IsMappedTo(other.Member.Name));
						if (selfMember.Attribute != null && referencedOther != null)
						{
							selfMember.IsReferenced = true;
							selfMember.MappedTo = referencedOther.PropertyName;
							continue;
						}

						// Other attribute references this self member
						var otherReferencingSelf = otherMembers.FirstOrDefault(other => other.Attribute?.IsMappedTo(selfMember.Member.Name) == true);
						if (otherReferencingSelf != null)
						{
							selfMember.IsReferenced = true;
							selfMember.MappedTo = otherReferencingSelf.PropertyName;
							continue;
						}
					}
				}
			}

			// Cleanup unmapped defs
			defsById = defsById
				.Where(x => x.Value.MapsToDefinition != null && x.Value.MemberDefinitions.Count > 0)
				.ToDictionary(x => x.Key, x => x.Value);

			// Finally cleanup unused members
			foreach (var def in defsById)
			{
				def.Value.MemberDefinitions = def.Value.MemberDefinitions.Where(x => x.IsReferenced).ToList();
			}

			return defsById.Values
				.OrderBy(x => x.Id)
				.ToDictionary(x => x.ClassType, x => x);
		}

		/// <summary>
		/// Formats definitions into left/right pairs.
		/// </summary>
		public static List<HCMappedClassesDefinition> CreateDefinitionPairs(IEnumerable<HCMappedClassDefinition> defs)
		{
			var defPairs = new Dictionary<string, HCMappedClassesDefinition>();
			foreach (var def in defs)
			{
				var orderedDefs = new[] { def, def.MapsToDefinition };
				if (orderedDefs.Any(x => x.Attribute?.Order != null)) orderedDefs = orderedDefs.OrderBy(x => x.Attribute?.Order ?? 0).ToArray();
				else orderedDefs = orderedDefs.OrderBy(x => x.TypeName).ToArray();

				var left = orderedDefs[0];
				var right = orderedDefs[1];
				var key = $"{left.Id}|{right.Id}";
				if (defPairs.ContainsKey(key)) continue;
				defPairs[key] = CreateMappedPair(left, right);
			}
			return defPairs
				.Values
				.Where(x => x.MemberPairs.Count > 0)
				.ToList();
		}

		private static HCMappedClassesDefinition CreateMappedPair(HCMappedClassDefinition left, HCMappedClassDefinition right)
		{
			static string createKey(HCMappedMemberDefinition[] defs)
				=> string.Join(", ", defs.Select(d => d.Id));

			var memberPairs = new Dictionary<string, HCMappedMemberDefinitionPair>();
			foreach (var leftMember in left.MemberDefinitions.Where(x => x.Attribute?.IsMappedToAnything == true))
			{
				var rightMembers = right.MemberDefinitions.Where(x => leftMember.Attribute.IsMappedTo(x.Id)).ToArray();
				if (rightMembers?.Any() != true) continue;
				var key = $"{leftMember.Id}|{createKey(rightMembers)}";
				memberPairs[key] = new HCMappedMemberDefinitionPair { Left = new[] { leftMember }, Right = rightMembers };
			}

			foreach (var rightMember in right.MemberDefinitions.Where(x => x.Attribute?.IsMappedToAnything == true))
			{
				var leftMembers = left.MemberDefinitions.Where(x => rightMember.Attribute.IsMappedTo(x.Id)).ToArray();
				if (leftMembers?.Any() != true) continue;
				var key = $"{createKey(leftMembers)}|{rightMember.Id}";
				memberPairs[key] = new HCMappedMemberDefinitionPair { Left = leftMembers, Right = new[] { rightMember } };
			}

			return new HCMappedClassesDefinition
			{
				Left = left,
				Right = right,
				MemberPairs = memberPairs.Values.ToList()
			};
		}

		private static HCMappedClassDefinition CreateDefinitionsFromType(Type type, Dictionary<string, HCMappedClassDefinition> defsById, HCMappedDefinitionDiscoveryOptions options,
			bool requireAttribute = true, string forcedMapsToDefinitionId = null, Type[] forceMapsToTypes = null, HCMappedClassDefinition parent = null)
		{
			var id = CreateMappedClassTypeId(type);
			if (defsById.TryGetValue(id, out var existing)) return existing;

			// Check all properties on the def for any of type containing a property with mappings.
			void createDefsFromSuitablePropertyTypes(HCMappedClassDefinition def)
			{
				if (def.MapsToType == null) return;
				foreach (var member in def.MemberDefinitions)
				{
					var memberType = member.Member.PropertyType;
					if (!memberType.GetProperties().Any(x => x.GetCustomAttribute<HCMappedPropertyAttribute>() != null)) continue;

					var otherProps = def.MapsToType?.GetProperties()
						.Where(x => member.Attribute?.IsMappedTo(x.Name) == true);
					if (otherProps?.Any() != true) continue;

					CreateDefinitionsFromType(memberType, defsById, options, requireAttribute: false, forceMapsToTypes: otherProps.Select(x => x.PropertyType).Distinct().ToArray(), parent: def);
					foreach (var otherProp in otherProps)
					{
						CreateDefinitionsFromType(otherProp.PropertyType, defsById, options, requireAttribute: false, forceMapsToTypes: new[] { memberType }, parent: def);
					}
				}
			}

			// Create def from input type if suitable
			var attr = type.GetCustomAttribute<HCMappedClassAttribute>();
			if (attr == null && requireAttribute) return null;
			var def = CreateClassDefinition(type, attr, options, forcedMapsToDefinitionId, forceMapsToTypes?.FirstOrDefault(), parent: parent);
			defsById[id] = def;
			createDefsFromSuitablePropertyTypes(def);

			// If def is referencing another type, include it.
			var mappedAgainstType = def.Attribute?.MappedToType;
			if (mappedAgainstType != null)
			{
				var mappedAgainstId = CreateMappedClassTypeId(mappedAgainstType);
				if (!defsById.ContainsKey(mappedAgainstId))
				{
					var mappedAgainstDef = CreateClassDefinition(mappedAgainstType, null, options, def.Id, parent: parent);
					defsById[mappedAgainstId] = mappedAgainstDef;
					createDefsFromSuitablePropertyTypes(mappedAgainstDef);
				}
			}

			return def;
		}

		private static HCMappedClassDefinition CreateClassDefinition(Type type, HCMappedClassAttribute attribute, HCMappedDefinitionDiscoveryOptions options,
			string forcedMapsToDefinitionId = null, Type forceMapsToType = null, HCMappedClassDefinition parent = null)
		{
			attribute ??= type.GetCustomAttribute<HCMappedClassAttribute>();

			var memberDefinitions = new List<HCMappedMemberDefinition>();
			foreach (var member in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var memberDef = CreateMemberDefinition(member, options);
				if (memberDef != null) memberDefinitions.Add(memberDef);
			}

			if (forceMapsToType != null && forcedMapsToDefinitionId == null)
			{
				forcedMapsToDefinitionId = CreateMappedClassTypeId(forceMapsToType);
			}

			return new HCMappedClassDefinition
			{
				Id = CreateMappedClassTypeId(type),
				MapsToDefinitionId = CreateMappedClassTypeId(attribute?.MappedToType) ?? forcedMapsToDefinitionId,
				MapsToType = attribute?.MappedToType ?? forceMapsToType,
				ClassType = type,
				GroupName = attribute?.GroupName ?? parent?.GroupName,
				Attribute = attribute,
				MemberDefinitions = memberDefinitions,
				TypeName = type.Name,
				DisplayName = attribute?.OverrideName ?? type.Name
			};
		}

		private static string CreateMappedClassTypeId(Type type) => type == null ? null : $"{type?.Namespace}.{type?.Name}";

		private static HCMappedMemberDefinition CreateMemberDefinition(PropertyInfo prop, HCMappedDefinitionDiscoveryOptions options)
		{
			var attributes = prop.GetCustomAttributes(true);
			var attribute = attributes.OfType<HCMappedPropertyAttribute>().FirstOrDefault();

			string autoDiscoveredName = null;
			if (options?.AllowAttributeDisplayNameResolve?.Invoke(prop) != false)
			{
				foreach (var attr in attributes)
				{
					var propertyNameProp = attr?.GetType()?.GetProperty("PropertyName") ?? attr?.GetType()?.GetProperty("Name");
					autoDiscoveredName = propertyNameProp?.GetValue(attr) as string;
					if (!string.IsNullOrWhiteSpace(autoDiscoveredName)) break;
				}
			}

			return new HCMappedMemberDefinition
			{
				Id = prop.Name,
				Member = prop,
				Attribute = attribute,
				PropertyName = prop.Name,
				DisplayName = options?.MemberDisplayNameOverride?.Invoke(prop) ?? attribute?.OverrideName ?? autoDiscoveredName ?? prop.Name
			};
		}
	}
}
