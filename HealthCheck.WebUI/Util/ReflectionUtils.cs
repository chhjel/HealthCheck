using System;
using System.Linq;
using System.Reflection;

namespace HealthCheck.WebUI.Util
{
    /// <summary>Utils to simplify life from HealthCheck tests and DCE.</summary>
    public static class ReflectionUtils
	{
		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TInstance">Type of object to invoke method on.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static object TryInvokeMethod<TInstance>(string methodName, params object[] parameters)
			where TInstance : class
			=> TryInvokeMethodExt<TInstance>(methodName, parameters);

		/// <summary>
		/// Attempt to invoke a method on the given type.
		/// <para>An instance will be attempted created.</para>
		/// </summary>
		/// <typeparam name="TInstance">Type of object to invoke method on.</typeparam>
		/// <param name="methodName">Name of the method to invoke</param>
		/// <param name="parameters">Method parameters if any</param>
		/// <param name="genericParameters">Generic method parameters if any</param>
		/// <returns>Method return value, or null if void.</returns>
		public static object TryInvokeMethodExt<TInstance>(string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			where TInstance : class
		{
			var instance = IoCUtils.GetInstanceExt<TInstance>();
			return TryInvokeMethodExt(instance?.GetType() ?? typeof(TInstance), instance, methodName, parameters, genericParameters);
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
		public static TReturn TryInvokeMethodExt<TInstance, TReturn>(string methodName,
			object[] parameters = null, Type[] genericParameters = null)
			where TInstance : class
		{
			var instance = IoCUtils.GetInstanceExt<TInstance>();
			return (TReturn)TryInvokeMethodExt(instance?.GetType() ?? typeof(TInstance), instance, methodName, parameters, genericParameters);
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
		/// <typeparam name="TInstance">Type of object to invoke method on.</typeparam>
		/// <param name="memberName">Name of the member to get the value from.</param>
		/// <returns>Value of the member.</returns>
		public static object TryGetMemberValue<TInstance>(string memberName)
			where TInstance : class
		{
			var instance = IoCUtils.GetInstanceExt<TInstance>();
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
				| BindingFlags.GetField
				| BindingFlags.FlattenHierarchy);

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
	}
}
