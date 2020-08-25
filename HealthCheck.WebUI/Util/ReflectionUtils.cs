using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NETFRAMEWORK
using System.Web.Mvc;
#endif

namespace HealthCheck.WebUI.Util
{
	/// <summary>Utils to simplify life from HealthCheck tests and DCE.</summary>
	public static class ReflectionUtils
	{
		/// <summary>
		/// Determines how to create new instances of types.
		/// <para>For .NET Framework this defaults to <c>DependencyResolver.Current.GetService</c></para>
		/// </summary>
		public static Func<Type, object> DefaultInstanceFactory { get; set; }
#if NETFRAMEWORK
		= DependencyResolver.Current.GetService;
#endif

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TInstance">Type of object to invoke method on.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <param name="genericParameters">Generic method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static object TryInvokeMethod<TInstance>(string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			where TInstance : class
		{
			var instance = TryGetInstance<TInstance>();
			return TryInvokeMethod(instance?.GetType() ?? typeof(TInstance), instance, methodName, parameters, genericParameters);
		}

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TInstance">Type of object to invoke method on.</typeparam>
		/// <typeparam name="TReturn">Type of the method return value.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <param name="genericParameters">Generic method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static TReturn TryInvokeMethod<TInstance, TReturn>(string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			where TInstance : class
		{
			var instance = TryGetInstance<TInstance>();
			return (TReturn)TryInvokeMethod(instance?.GetType() ?? typeof(TInstance), instance, methodName, parameters, genericParameters);
		}

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// </summary>
		/// <param name="instance">Object instance to invoke method on</param>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <param name="genericParameters">Generic method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static object TryInvokeMethod(object instance, string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			=> TryInvokeMethod(instance.GetType(), instance, methodName, parameters, genericParameters);

		private static object TryInvokeMethod(Type type, object instance, string methodName,
			object[] parameters = null, Type[] genericParameters = null)
		{
			var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
			parameters = parameters ?? new object[0];
			genericParameters = genericParameters ?? new Type[0];

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
		/// <typeparam name="TInstance">Type of object to invoke method on.</typeparam>
		/// <param name="memberName">Name of the member to get the value from.</param>
		/// <returns>Value of the member.</returns>
		public static object TryGetMemberValue<TInstance>(string memberName)
			where TInstance : class
		{
			var instance = TryGetInstance<TInstance>();
			return TryGetMemberValue(instance?.GetType() ?? typeof(TInstance), instance, memberName);
		}

		/// <summary>
		/// Attempt to get the value of a member.
		/// </summary>
		/// <param name="instance">Object instance to get the value from.</param>
		/// <param name="memberName">Name of the member to get the value from.</param>
		/// <returns>Value of the member.</returns>
		public static object TryGetMemberValue(object instance, string memberName)
			=> TryGetMemberValue(instance.GetType(), instance, memberName);

		private static object TryGetMemberValue(Type type, object instance, string memberName)
		{
			var members = type.GetMembers(
				BindingFlags.NonPublic | BindingFlags.Public
				| BindingFlags.Static | BindingFlags.Instance
				| BindingFlags.GetProperty
				| BindingFlags.GetField);

			var member = members.FirstOrDefault(x => x.Name == memberName);
			return GetMemberValue(member, instance);
		}

		private static object GetMemberValue(this MemberInfo memberInfo, object instance)
		{
			switch (memberInfo.MemberType)
			{
				case MemberTypes.Field:
					return ((FieldInfo)memberInfo).GetValue(instance);
				case MemberTypes.Property:
					return ((PropertyInfo)memberInfo).GetValue(instance);
				default:
					return null;
			}
		}

		/// <summary>
		/// Attempts to create get an instance of the given type.
		/// </summary>
		/// <typeparam name="T">Type to create or get.</typeparam>
		/// <param name="instanceFactory">Defaults to <see cref="DefaultInstanceFactory"/></param>
		/// <param name="forcedParameterValues">Optionally force any constructor parameter values by name.</param>
		/// <returns></returns>
		public static T TryGetInstance<T>(
			Func<Type, object> instanceFactory = null,
			Dictionary<string, object> forcedParameterValues = null
		)
			where T : class
		{
			try
			{
				var type = typeof(T);
				instanceFactory ??= DefaultInstanceFactory;

				if (type.IsInterface)
				{
					return instanceFactory?.Invoke(type) as T;
				}

				var constructor = type.GetConstructors()
					.OrderBy(x => x.GetParameters().Length)
					.FirstOrDefault();

				if (constructor == null)
				{
					return Activator.CreateInstance(type) as T;
				}

				var parameters = new List<object>();
				foreach (var parameter in constructor.GetParameters())
				{
					object parameterInstance = forcedParameterValues?.ContainsKey(parameter.Name) == true
						? forcedParameterValues[parameter.Name]
						: instanceFactory?.Invoke(parameter.ParameterType);

					parameters.Add(parameterInstance);
				}
				return constructor.Invoke(parameters.ToArray()) as T;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
