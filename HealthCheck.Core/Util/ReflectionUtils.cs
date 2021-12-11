using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HealthCheck.Core.Util
{
	/// <summary>Utils to simplify life from HealthCheck tests and DCE.</summary>
	public static class ReflectionUtils
	{
		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static object TryInvokeMethod<TClass>(string methodName, params object[] parameters)
			where TClass : class
			=> TryInvokeMethodExt<TClass>(methodName, parameters);

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
		/// <typeparam name="TResult">Type the result will be cast to.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static TResult TryInvokeMethod<TClass, TResult>(string methodName, params object[] parameters)
			where TClass : class
			=> (TResult)TryInvokeMethodExt<TClass>(methodName, parameters);

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <param name="genericParameters">Generic method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static object TryInvokeMethodExt<TClass>(string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			where TClass : class
		{
			var instance = IoCUtils.GetInstanceExt<TClass>();
			return TryInvokeMethodExt(instance?.GetType() ?? typeof(TClass), instance, methodName, parameters, genericParameters);
		}

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
		/// <typeparam name="TReturn">Type of the method return value.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <param name="genericParameters">Generic method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static TReturn TryInvokeMethodExt<TClass, TReturn>(string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			where TClass : class
		{
			var instance = IoCUtils.GetInstanceExt<TClass>();
			return (TReturn)TryInvokeMethodExt(instance?.GetType() ?? typeof(TClass), instance, methodName, parameters, genericParameters);
		}

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// </summary>
		/// <param name="instance">Object instance to invoke method on</param>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <param name="genericParameters">Generic method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static object TryInvokeMethodExt(object instance, string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			=> TryInvokeMethodExt(instance.GetType(), instance, methodName, parameters, genericParameters);

		private static object TryInvokeMethodExt(Type type, object instance, string methodName,
			object[] parameters = null, Type[] genericParameters = null)
		{
			var methods = type.GetMethods(
				BindingFlags.NonPublic 
				| BindingFlags.Public 
				| BindingFlags.Static 
				| BindingFlags.Instance
				| BindingFlags.FlattenHierarchy);
			parameters ??= new object[0];
			genericParameters ??= new Type[0];

			var method = methods.FirstOrDefault(x =>
				x.Name == methodName
				&& x.GetParameters().Length == parameters.Length
				&& (
					(!x.ContainsGenericParameters && genericParameters.Length == 0)
					||
					(x.ContainsGenericParameters && x.GetGenericArguments().Length == genericParameters.Length)
				)
			);

			if (method?.IsGenericMethod == true)
			{
				method = method.MakeGenericMethod(genericParameters);
			}
			return method?.Invoke(instance, parameters);
		}

		/// <summary>
		/// Attempt to get the value of a member.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
		/// <param name="memberName">Name of the member to get the value from.</param>
		/// <returns>Value of the member.</returns>
		public static object TryGetMemberValue<TClass>(string memberName)
			where TClass : class
		{
			var instance = IoCUtils.GetInstanceExt<TClass>();
			return TryGetMemberValue(instance?.GetType() ?? typeof(TClass), instance, memberName);
		}

		/// <summary>
		/// Attempt to get the value of a member.
		/// </summary>
		/// <param name="instance">Object instance to get the value from.</param>
		/// <param name="memberName">Name of the member to get the value from.</param>
		/// <returns>Value of the member.</returns>
		public static object TryGetMemberValue(object instance, string memberName)
			=> TryGetMemberValue(instance.GetType(), instance, memberName);

		/// <summary>
		/// Attempts to create a new instance of the given type, ignoring any errors and returning null on failure.
		/// </summary>
		public static object TryActivate(Type type)
		{
			try
			{
				return Activator.CreateInstance(type);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Name and type of a member.
		/// </summary>
		public class TypeMemberData
		{
			/// <summary>
			/// Name of the member or its path.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Type of the member.
			/// </summary>
			public Type Type { get; set; }
		}

		/// <summary>
		/// Get a list of members recursively.
		/// <para><see cref="TypeMemberData.Name" /> will be the dotted path to the member.</para>
		/// </summary>
		public static List<TypeMemberData> GetTypeMembersRecursive(Type type, string path = null, int currentLevel = 0, int maxLevels = 10,
			List<TypeMemberData> worklist = null, HashSet<Type> ignoredTypes = null)
		{
			if (type == null) return new();

			var paths = worklist ?? new List<TypeMemberData>();
			if (currentLevel >= maxLevels) return paths;

			ignoredTypes ??= new();
			//ignoredTypes.Add(type);

			bool allowRecurseType(Type type)
			{
				return !ignoredTypes.Contains(type)
					&& !type.IsSpecialName
					&& !type.IsValueType
					&& !type.IsPrimitive
					&& !type.IsArray
					&& type.Namespace?.StartsWith("System.") != true
					&& type.Namespace != "System"
					&& type.Module.ScopeName != "CommonLanguageRuntimeLibrary"
					&& !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
					&& !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					&& !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>));
			}

			var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			var props = type.GetProperties(bindingFlags);
			foreach (var prop in props)
			{
				paths.Add(new TypeMemberData
				{
					Name = $"{(path == null ? "" : $"{path}.")}{prop.Name}",
					Type = prop.PropertyType
				});

				if (!prop.IsSpecialName
					&& prop.GetMethod != null
					&& prop.CanRead
					&& allowRecurseType(prop.PropertyType)
					&& prop.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
				{
					GetTypeMembersRecursive(prop.PropertyType, $"{(string.IsNullOrWhiteSpace(path) ? "" : $"{path}.")}{prop.Name}", currentLevel + 1, maxLevels, paths, ignoredTypes);
				}
			}

			var fields = type.GetFields();
			foreach (var field in fields)
			{
				paths.Add(new TypeMemberData
				{
					Name = $"{(path == null ? "" : $"{path}.")}{field.Name}",
					Type = field.FieldType
				});

				if (!field.IsSpecialName
					&& allowRecurseType(field.FieldType)
					&& field.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
				{
					GetTypeMembersRecursive(field.FieldType, $"{(string.IsNullOrWhiteSpace(path) ? "" : $"{path}.")}{field.Name}", currentLevel + 1, maxLevels, paths, ignoredTypes);
				}
			}

			return paths;
		}

		/// <summary>
		/// Get a property by its dotted path.
		/// <para>Returns null if not found.</para>
		/// </summary>
		public static object GetValue(object rootInstance, string path)
		{
			var instance = rootInstance;
			if (instance == null) return null;

			object getValue(string membName, out bool found)
			{
				found = false;
				if (instance == null) return null;

				var prop = instance.GetType().GetProperty(membName);
				if (prop != null)
				{
					found = true;
					return prop.GetValue(instance);
				}

				var field = instance.GetType().GetField(membName);
				if (field != null)
				{
					found = true;
					return field.GetValue(instance);
				}

				return null;
			}

			while (path.Contains("."))
			{
				var memberName = path.Substring(0, path.IndexOf("."));
				path = path.Substring(memberName.Length + 1);

				instance = getValue(memberName, out var wasFound);
				if (!wasFound || instance == null)
				{
					return null;
				}
			}

            return getValue(path, out _);
		}

		private static object TryGetMemberValue(Type type, object instance, string memberName)
		{
			var members = type.GetMembers(
				BindingFlags.NonPublic | BindingFlags.Public
				| BindingFlags.Static | BindingFlags.Instance
				| BindingFlags.GetProperty
				| BindingFlags.GetField
				| BindingFlags.FlattenHierarchy);

			var member = members.FirstOrDefault(x => x.Name == memberName);
			return GetMemberValue(member, instance);
		}

		private static object GetMemberValue(this MemberInfo memberInfo, object instance)
		{
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(instance),
                MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(instance),
                _ => null,
            };
        }
	}
}
