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
    public static class IoCUtils
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
		/// Attempts to get an instance of the given type.
		/// <para>Shortcut to GetInstanceExt&lt;T&gt;(forcedParameterValues: ..)</para>
		/// </summary>
		/// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
		public static T GetInstance<T>(params object[] forcedParameterValues)
			where T : class
			=> GetInstanceExt<T>(forcedParameterValues: forcedParameterValues);

        /// <summary>
        /// Attempts to get an instance of the given type.
        /// </summary>
        /// <typeparam name="T">Type to create or get.</typeparam>
        /// <param name="instanceFactory">Defaults to <see cref="DefaultInstanceFactory"/></param>
        /// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
        /// <param name="recursive">Any parameter values in constructors in injected parameters will also be using forced parameter values if enabled.</param>
        public static T GetInstanceExt<T>(
			Func<Type, object> instanceFactory = null,
			object[] forcedParameterValues = null,
			bool recursive = true
		)
			where T : class
			=> GetInstanceExt(typeof(T), instanceFactory, forcedParameterValues, recursive) as T;

		/// <summary>
		/// Attempts to get an instance of the given type.
		/// </summary>
		/// <param name="type">Type to create or get.</param>
		/// <param name="instanceFactory">Defaults to <see cref="DefaultInstanceFactory"/></param>
		/// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
		/// <param name="recursive">Any parameter values in constructors in injected parameters will also be using forced parameter values if enabled.</param>
		public static object GetInstanceExt(
			Type type,
			Func<Type, object> instanceFactory = null,
			object[] forcedParameterValues = null,
			bool recursive = true
		)
		{
			instanceFactory ??= DefaultInstanceFactory;

			if (type.IsInterface)
			{
				// If recursive get type and GetInstanceExt it
				if (recursive)
				{
					var instance = instanceFactory?.Invoke(type);
					if (instance != null)
					{
						var instanceType = instance.GetType();
						return GetInstanceExt(
							instanceType, instanceFactory,
							forcedParameterValues, recursive);
					}
				}
				return instanceFactory?.Invoke(type);
			}

			var constructor = GetConstructor(type);
			if (constructor == null)
			{
				return Activator.CreateInstance(type);
			}

			// Build forced parameters data by joining by name and by value parameters
			var constructorParameters = constructor.GetParameters();
			var forcedParameterValuesByName = BuildNamedParametersList(constructorParameters, forcedParameterValues);

			// Build paramaters to pass to constructor
			var parameters = new List<object>();
			foreach (var parameter in constructorParameters)
			{
				object parameterInstance = null;
				if (forcedParameterValuesByName?.ContainsKey(parameter.Name) == true)
				{
					parameterInstance = forcedParameterValuesByName[parameter.Name];
				}
				else if (recursive)
				{
					parameterInstance = GetInstanceExt(
						parameter.ParameterType, instanceFactory,
						forcedParameterValues, recursive);
				}
				else
				{
					parameterInstance = instanceFactory?.Invoke(parameter.ParameterType);
				}

				parameters.Add(parameterInstance);
			}

			return constructor.Invoke(parameters.ToArray());
		}

		private static Dictionary<string, object> BuildNamedParametersList(ParameterInfo[] parameters, object[] instances)
		{
			var forcedParameterValuesByName = new Dictionary<string, object>();
			if (instances == null)
			{
				return forcedParameterValuesByName;
			}

			foreach (var forcedParameter in instances)
			{
				var parameterName = parameters.FirstOrDefault(x =>
						x.ParameterType.IsInstanceOfType(forcedParameter)
					)?.Name;
				if (parameterName == null)
				{
					continue;
				}

				forcedParameterValuesByName[parameterName] = forcedParameter;
			}
			return forcedParameterValuesByName;
		}

		private static ConstructorInfo GetConstructor(Type type)
			=> type.GetConstructors()
				.OrderBy(x => x.GetParameters().Length)
				.FirstOrDefault();
	}
}
