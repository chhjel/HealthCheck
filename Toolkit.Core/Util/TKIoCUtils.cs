using QoDL.Toolkit.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Utils to simplify life from Toolkit tests and DCE.
    /// <para>The default instance resolver can be configured through <see cref="TKGlobalConfig.DefaultInstanceResolver"/></para>
    /// </summary>
    public static class TKIoCUtils
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
        /// <param name="instanceFactory">Defaults to <see cref="TKGlobalConfig.DefaultInstanceResolver"/></param>
        /// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
        /// <param name="recursive">Any parameter values in constructors in injected parameters will also be using forced parameter values if enabled.</param>
        /// <param name="scope">Optional ioc scope</param>
        public static TClass GetInstanceExt<TClass>(
            TKGlobalConfig.InstanceResolverDelegate instanceFactory = null,
            object[] forcedParameterValues = null,
            bool recursive = true,
            ScopeContainer scope = null
        )
            where TClass : class
            => GetInstanceExt(typeof(TClass), instanceFactory, forcedParameterValues, recursive, 20, scope: scope) as TClass;

        /// <summary>
        /// Attempts to get an instance of the given type.
        /// </summary>
        /// <param name="type">Type to create or get.</param>
        /// <param name="instanceFactory">Defaults to <see cref="TKGlobalConfig.DefaultInstanceResolver"/></param>
        /// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
        /// <param name="recursive">Any parameter values in constructors in injected parameters will also be using forced parameter values if enabled.</param>
        /// <param name="maxDepth">Max recursive depth.</param>
        /// <param name="scope">Optional ioc scope</param>
        public static object GetInstanceExt(
            Type type,
            TKGlobalConfig.InstanceResolverDelegate instanceFactory = null,
            object[] forcedParameterValues = null,
            bool recursive = true,
            int maxDepth = 20,
            ScopeContainer scope = null
        ) => GetInstanceExt(type, instanceFactory, forcedParameterValues, recursive, maxDepth, 0, scope: scope).Instance;

        /// <summary>
        /// Attempts to get an instance of the given type.
        /// </summary>
        /// <param name="type">Type to create or get.</param>
        /// <param name="instanceFactory">Defaults to <see cref="TKGlobalConfig.DefaultInstanceResolver"/></param>
        /// <param name="forcedParameterValues">Any values used here will be used on the first matching constructor parameter if any.</param>
        /// <param name="recursive">Any parameter values in constructors in injected parameters will also be using forced parameter values if enabled.</param>
        /// <param name="maxDepth">Max recursive depth.</param>
        /// <param name="scope">Optional ioc scope</param>
        public static ActivatedInstanceResult GetInstanceExtWithDetails(
            Type type,
            TKGlobalConfig.InstanceResolverDelegate instanceFactory = null,
            object[] forcedParameterValues = null,
            bool recursive = true,
            int maxDepth = 20,
            ScopeContainer scope = null
        ) => GetInstanceExt(type, instanceFactory, forcedParameterValues, recursive, maxDepth, 0, scope: scope);

        private static ActivatedInstanceResult GetInstanceExt(
            Type type,
            TKGlobalConfig.InstanceResolverDelegate instanceFactory,
            object[] forcedParameterValues,
            bool recursive,
            int maxDepth,
            int currentDepth,
            ScopeContainer scope = null
        )
        {
            if (currentDepth >= maxDepth)
            {
                return new ActivatedInstanceResult
                {
                    ErrorDetails = $"Max IoC recursion depth reached (depth: {maxDepth}, type: '{type.Name}')"
                };
            }

            if (type.IsValueType || type.IsPrimitive || type == typeof(object))
            {
                return new ActivatedInstanceResult
                {
                    ErrorDetails = $"Can't inject value type, primitive or object. Attempted {type.Name}."
                };
            }

            scope ??= new ScopeContainer();
            instanceFactory ??= TKGlobalConfig.GetDefaultInstanceResolver();

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
                            var instanceResult = GetInstanceExt(
                                instanceType, instanceFactory,
                                forcedParameterValues, recursive,
                                maxDepth, currentDepth + 1,
                                scope: scope);
                            if (instanceResult.Instance != null)
                            {
                                return instanceResult;
                            }
                        }
                        catch (Exception) { /* Ignore recursive errors */ }
                    }
                }
                var instance = instanceFactory?.Invoke(type, scope);
                return new ActivatedInstanceResult
                {
                    Success = true,
                    Instance = instance
                };
            }

            var constructor = GetConstructor(type);
            if (constructor == null)
            {
                var instance = instanceFactory?.Invoke(type) ?? Activator.CreateInstance(type);
                return new ActivatedInstanceResult
                {
                    Success = true,
                    Instance = instance
                };
            }

            // Build forced parameters data by joining by name and by value parameters
            var constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 0)
            {
                var instance = instanceFactory?.Invoke(type) ?? Activator.CreateInstance(type);
                return new ActivatedInstanceResult
                {
                    Success = true,
                    Instance = instance
                };
            }
            else if (constructorParameters.Any(x => x.ParameterType == typeof(object)))
            {
                return new ActivatedInstanceResult
                {
                    ErrorDetails = $"Constructor on type '{type.Name}' contains parameter of type object."
                };
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
                        maxDepth, currentDepth + 1).Instance
                        ?? instanceFactory.Invoke(parameter.ParameterType);
                }
                else
                {
                    parameterInstance = instanceFactory?.Invoke(parameter.ParameterType);
                }

                parameters.Add(parameterInstance);
            }

            var ctoredInstance = constructor.Invoke(parameters.ToArray());
            return new ActivatedInstanceResult
            {
                Success = true,
                Instance = ctoredInstance
            };
        }

        /// <summary>
        /// Container for optional scope object.
        /// </summary>
        public class ScopeContainer
        {
            /// <summary>
            /// Optional ioc scope object.
            /// </summary>
            public object Scope { get; set; }
        }


        /// <summary>
        /// Wrapper containing success/error/instance data.
        /// </summary>
        public struct ActivatedInstanceResult
        {
            /// <summary>
            /// True if the type was activated.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Details if any.
            /// </summary>
            public string ErrorDetails { get; set; }

            /// <summary>
            /// Activated instance.
            /// </summary>
            public object Instance { get; set; }
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
