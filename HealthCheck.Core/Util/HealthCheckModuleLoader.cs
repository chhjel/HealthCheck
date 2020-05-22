using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util
{
	/// <summary>
	/// Handles loading of modules.
	/// </summary>
	public class HealthCheckModuleLoader
	{
		/// <summary>
		/// Load the given module.
		/// </summary>
		public HealthCheckLoadedModule Load(IHealthCheckModule module, object accessOptions, string name = null)
		{
			var type = module.GetType();
			var loadedModule = new HealthCheckLoadedModule
			{
				Type = type,
				Module = module,
				Name = name
			};

			try
			{
				loadedModule.AccessOptionsType = GetModuleAccessOptionsType(type);
				if (loadedModule.AccessOptionsType == null)
				{
					loadedModule.LoadErrors.Add($"Access options type could not be determined.");
				}
				else if (loadedModule.AccessOptionsType != accessOptions?.GetType())
				{
					loadedModule.LoadErrors.Add($"Given access options type does not match the module access options type.");
				}
				else if (!EnumUtils.IsTypeEnumFlag(loadedModule.AccessOptionsType))
				{
					loadedModule.LoadErrors.Add($"Access options type should be decorated with [Flags].");
				}

				var getOptionsMethod = type.GetMethods()
					.FirstOrDefault(x => x.Name == nameof(HealthCheckModuleBase<StringSplitOptions>.GetFrontendOptionsObject)
										&& x.GetParameters().Length == 1);
				var getConfigMethod = type.GetMethods()
					.FirstOrDefault(x => x.Name == nameof(HealthCheckModuleBase<StringSplitOptions>.GetModuleConfig)
										&& x.GetParameters().Length == 1
										&& typeof(IHealthCheckModuleConfig).IsAssignableFrom(x.ReturnType));

				loadedModule.Config = getConfigMethod.Invoke(module, new object[] { accessOptions }) as IHealthCheckModuleConfig;
				loadedModule.FrontendOptions = getOptionsMethod.Invoke(module, new object[] { accessOptions });

				if (loadedModule.Config == null)
				{
					loadedModule.LoadErrors.Add($"GetModuleConfig should return something not null.");
				}
				else
				{
					ValidateConfigValues(loadedModule);
				}

				if (loadedModule.Config != null && name == null)
				{
					loadedModule.Name = loadedModule?.Config?.Name ?? name;
				}

				loadedModule.InvokableMethods = type.GetMethods()
					.Where(x =>
						x.GetCustomAttributes(true)
						.Any(a => typeof(HealthCheckModuleMethodAttribute).IsAssignableFrom(a.GetType()))
					)
					.Select(x => new InvokableMethod(x))
					.ToList();

				var dupeMethods = loadedModule.InvokableMethods.GroupBy(x => x.Name).Where(x => x.Count() > 1).ToArray();
				if (dupeMethods.Any())
				{
					loadedModule.LoadErrors.Add($"Contains multiple invokable methods with the same name. Please give them different names. [{string.Join(", ", dupeMethods.Select(x => x.First().Name))}]");
				}
			}
			catch (Exception ex)
			{
				loadedModule.LoadErrors.Add($"Exception occured on module load: {ex.Message}");
				loadedModule.LoadErrorStacktrace = ex?.ToString();
			}

			return loadedModule;
		}

		private static void ValidateConfigValues(HealthCheckLoadedModule loadedModule)
		{
			var invalidProps = new List<string>();
			if (loadedModule.Config.DefaultRootRouteSegment == null) invalidProps.Add(nameof(IHealthCheckModuleConfig.DefaultRootRouteSegment));
			if (loadedModule.Config.InitialRoute == null) invalidProps.Add(nameof(IHealthCheckModuleConfig.InitialRoute));
			if (loadedModule.Config.RoutePath == null) invalidProps.Add(nameof(IHealthCheckModuleConfig.RoutePath));
			if (loadedModule.Config.ComponentName == null) invalidProps.Add(nameof(IHealthCheckModuleConfig.ComponentName));

			foreach (var prop in invalidProps)
			{
				loadedModule.LoadErrors.Add($"Config property '{prop}' should not be null.");
			}
		}

		/// <summary>
		/// Get the generic argument type of the module.
		/// </summary>
		public static Type GetModuleAccessOptionsType(Type moduleType)
		{
			if (!typeof(IHealthCheckModule).IsAssignableFrom(moduleType))
			{
				throw new ArgumentException($"Can't find module access options type, '{moduleType.Name}' is not assignable to {nameof(IHealthCheckModule)}.");
			}

			for (int i = 0; i < 100; i++)
			{
				var baseType = moduleType.BaseType;
				if (baseType == null)
				{
					return null;
				}

				if (baseType.IsGenericType == true && baseType.GetGenericTypeDefinition() == typeof(HealthCheckModuleBase<>))
				{
					return baseType.GetGenericArguments()[0];
				}

				moduleType = baseType;
			}
			return null;
		}

		/// <summary>
		/// A loaded module.
		/// </summary>
		public class HealthCheckLoadedModule
		{
			/// <summary>
			/// Unique id of the module.
			/// </summary>
			public string Id => Type.Name;

			/// <summary>
			/// Type of the module.
			/// </summary>
			public Type Type { get; set; }

			/// <summary>
			/// Type of the access options enum.
			/// </summary>
			public Type AccessOptionsType { get; set; }

			/// <summary>
			/// True if no load errors.
			/// </summary>
			public bool LoadedSuccessfully => !LoadErrors.Any();

			/// <summary>
			/// Any detected errors during load.
			/// </summary>
			public List<string> LoadErrors { get; set; } = new List<string>();

			/// <summary>
			/// Full stack-trace if any.
			/// </summary>
			public string LoadErrorStacktrace { get; set; }

			/// <summary>
			/// Name of the module.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Module config object.
			/// </summary>
			public IHealthCheckModuleConfig Config { get; set; }

			/// <summary>
			/// Module frontend options object.
			/// </summary>
			public object FrontendOptions { get; set; }

			/// <summary>
			/// List of methods on the module that can be invoked from frontend.
			/// </summary>
			public List<InvokableMethod> InvokableMethods { get; set; }

			/// <summary>
			/// The module that was loaded.
			/// </summary>
			public IHealthCheckModule Module { get; set; }
		}

		/// <summary>
		/// A method on a module that can be invoked from frontend.
		/// </summary>
		public class InvokableMethod
		{
			/// <summary>
			/// Name of the method.
			/// </summary>
			public string Name => Method.Name;

			/// <summary>
			/// True if the method returns a task.
			/// </summary>
			public bool IsAsync { get; set; }

			/// <summary>
			/// Enum values required to invoke this method.
			/// </summary>
			public object RequiresAccessTo { get; set; }

			/// <summary>
			/// Type of the parameter.
			/// </summary>
			public Type ParameterType { get; set; }

			/// <summary>
			/// True if there is any parameters.
			/// </summary>
			public bool HasParameterType => ParameterType != null;

			/// <summary>
			/// True if the first parameter is a <see cref="HealthCheckModuleContext"/>.
			/// </summary>
			public bool HasContextParameter { get; set; }

			/// <summary>
			/// Return type from the method.
			/// </summary>
			public Type ReturnType { get; set; }

			/// <summary>
			/// True if the method is not void or returns an empty Task.
			/// </summary>
			public bool HasReturnType => ReturnType != null;

			private MethodInfo Method { get; set; }

			/// <summary>
			/// A method on a module that can be invoked from frontend.
			/// </summary>
			public InvokableMethod(MethodInfo method)
			{
				Method = method;
				var attribute = method.GetCustomAttributes(true)
					.FirstOrDefault(a => typeof(HealthCheckModuleMethodAttribute).IsAssignableFrom(a.GetType()))
					as HealthCheckModuleMethodAttribute;

				RequiresAccessTo = attribute.RequiresAccessTo;

				IsAsync = MethodIsAsync(method);
				ReturnType = GetReturnType(method);

				var parameters = method.GetParameters();
				if (parameters.Length > 0)
				{
					HasContextParameter = parameters[0].ParameterType == typeof(HealthCheckModuleContext);
					if (parameters.Length > 1 || !HasContextParameter)
					{
						ParameterType = parameters.Last().ParameterType;
					}
				}
			}

			/// <summary>
			/// Invoke the method with the given serialized parameter. Returns a serialized response.
			/// </summary>
			public async Task<string> Invoke(IHealthCheckModule instance, HealthCheckModuleContext context, string jsonPayload, IJsonSerializer serializer)
			{
				List<object> parameters = new List<object>();
				if (HasContextParameter)
				{
					parameters.Add(context);
				}
				if (HasParameterType)
				{
					parameters.Add(serializer.Deserialize(jsonPayload, ParameterType));
				}

				var result = Method.Invoke(instance, (parameters.Count == 0) ? null : parameters.ToArray());
				if (result is Task resultTask)
				{
					await resultTask.ConfigureAwait(false);

					var resultProperty = resultTask.GetType().GetProperty("Result");
					result = resultProperty.GetValue(resultTask);
				}

				return (HasReturnType) ? serializer.Serialize(result) : null;
			}

			private static Type GetReturnType(MethodInfo method)
			{
				var type = (method.ReturnType == typeof(void) || method.ReturnType == typeof(Task)) ? null : method.ReturnType;
				if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
				{
					type = type.GetGenericArguments()[0];
				}
				return type;
			}

			private static bool MethodIsAsync(MethodInfo method)
			{
				var returnType = method.ReturnType;
				if (returnType == typeof(Task))
				{
					return true;
				}
				else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				{
					return true;
				}

				return false;
			}
		}
	}
}
