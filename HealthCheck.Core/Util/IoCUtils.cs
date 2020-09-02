﻿using HealthCheck.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Utils to simplify life from HealthCheck tests and DCE.
    /// <para>The default instance resolver can be configured through <see cref="HCGlobalConfig.DefaultInstanceResolver"/></para>
    /// </summary>
    public static class IoCUtils
    {
        /// <summary>
        /// Attempts to get an instance of the given type.
        /// <para>Shortcut to GetInstanceExt&lt;T&gt;(forcedParameterValues: ..)</para>
        /// </summary>
        /// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
        public static TClass GetInstance<TClass>(params object[] forcedParameterValues)
            where TClass : class
            => GetInstanceExt<TClass>(forcedParameterValues: forcedParameterValues);

        /// <summary>
        /// Attempts to get an instance of the given type.
        /// </summary>
        /// <typeparam name="TClass">Type to create or get.</typeparam>
        /// <param name="instanceFactory">Defaults to <see cref="HCGlobalConfig.DefaultInstanceResolver"/></param>
        /// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
        /// <param name="recursive">Any parameter values in constructors in injected parameters will also be using forced parameter values if enabled.</param>
        public static TClass GetInstanceExt<TClass>(
            Func<Type, object> instanceFactory = null,
            object[] forcedParameterValues = null,
            bool recursive = true
        )
            where TClass : class
            => GetInstanceExt(typeof(TClass), instanceFactory, forcedParameterValues, recursive) as TClass;

        /// <summary>
        /// Attempts to get an instance of the given type.
        /// </summary>
        /// <param name="type">Type to create or get.</param>
        /// <param name="instanceFactory">Defaults to <see cref="HCGlobalConfig.DefaultInstanceResolver"/></param>
        /// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
        /// <param name="recursive">Any parameter values in constructors in injected parameters will also be using forced parameter values if enabled.</param>
        /// <param name="maxDepth">Max recursive depth.</param>
        public static object GetInstanceExt(
            Type type,
            Func<Type, object> instanceFactory = null,
            object[] forcedParameterValues = null,
            bool recursive = true,
            int maxDepth = 10
        ) => GetInstanceExt(type, instanceFactory, forcedParameterValues, recursive, maxDepth, 0);

        private static object GetInstanceExt(
            Type type,
            Func<Type, object> instanceFactory,
            object[] forcedParameterValues,
            bool recursive,
            int maxDepth,
            int currentDepth
        )
        {
            if (currentDepth >= maxDepth)
            {
                return null;
            }

            if (type.IsValueType || type.IsPrimitive || type == typeof(object))
            {
                return null;
            }

            instanceFactory ??= HCGlobalConfig.GetDefaultInstanceResolver();

            if (type.IsInterface)
            {
                // If recursive get type and GetInstanceExt it
                if (recursive && (forcedParameterValues != null && forcedParameterValues.Length > 0))
                {
                    var defInstance = instanceFactory?.Invoke(type);
                    if (defInstance != null)
                    {
                        var instanceType = defInstance.GetType();
                        try
                        {
                            var instance = GetInstanceExt(
                                instanceType, instanceFactory,
                                forcedParameterValues, recursive,
                                maxDepth, currentDepth + 1);
                            if (instance != null)
                            {
                                return instance;
                            }
                        }
                        catch (Exception) { /* Ignore recursive errors */ }
                    }
                }
                return instanceFactory?.Invoke(type);
            }

            var constructor = GetConstructor(type);
            if (constructor == null)
            {
                return instanceFactory?.Invoke(type);
            }

            // Build forced parameters data by joining by name and by value parameters
            var constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 0)
            {
                return instanceFactory.Invoke(type);
            }
            else if (constructorParameters.Any(x => x.ParameterType == typeof(object)))
            {
                return null;
            }

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
                        forcedParameterValues, recursive,
                        maxDepth, currentDepth + 1)
                        ?? instanceFactory.Invoke(parameter.ParameterType);
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
                        x.ParameterType != typeof(object)
                        && x.ParameterType.IsInstanceOfType(forcedParameter)
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
