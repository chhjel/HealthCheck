using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Abstractions.Modules;
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
		/// Load the given modules.
		/// </summary>
		public List<HealthCheckLoadedModule> Load(List<IHealthCheckModule> modules)
		{
			return modules
				.Select(x => Load(x))
				.Where(x => x != null)
				.ToList();
		}

		/// <summary>
		/// Load the given module.
		/// </summary>
		public HealthCheckLoadedModule Load(IHealthCheckModule module)
		{
			var type = module.GetType();
			var loadedModule = new HealthCheckLoadedModule
			{
				Type = type,
				Module = module
			};

			try
			{
				loadedModule.AccessOptionsType = GetModuleAccessOptionsType(type);
				if (loadedModule.AccessOptionsType == null)
				{
					loadedModule.LoadErrors.Add($"Access options type could not be determined.");
				}
				else if (!EnumUtils.IsTypeEnumFlag(loadedModule.AccessOptionsType))
				{
					loadedModule.LoadErrors.Add($"Access options type should be decorated with [Flags].");
				}

				var getOptionsMethod = type.GetMethods()
					.FirstOrDefault(x => x.Name == "GetFrontendOptionsObject" && x.GetParameters().Length == 0);
				var getConfigMethod = type.GetMethods()
					.FirstOrDefault(x => x.Name == "GetModuleConfig" && x.GetParameters().Length == 0
						&& typeof(IHealthCheckModuleConfig).IsAssignableFrom(x.ReturnType));

				loadedModule.Config = getConfigMethod.Invoke(module, new object[0]) as IHealthCheckModuleConfig;
				loadedModule.FrontendOptions = getOptionsMethod.Invoke(module, new object[0]);

				if (loadedModule.Config == null)
				{
					loadedModule.LoadErrors.Add($"GetModuleConfig should return something not null.");
				}

				loadedModule.InvokableMethods = type.GetMethods()
					.Where(x =>
						x.GetCustomAttributes(true)
						.Any(a => typeof(HealthCheckModuleAccessAttribute).IsAssignableFrom(a.GetType()))
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

		private Type GetModuleAccessOptionsType(Type moduleType)
		{
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
			public string Id => Type.FullName;

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
			public string Name => Config?.Name;

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
					.FirstOrDefault(a => typeof(HealthCheckModuleAccessAttribute).IsAssignableFrom(a.GetType()))
					as HealthCheckModuleAccessAttribute;

				RequiresAccessTo = attribute.RequiresAccessTo;

				IsAsync = MethodIsAsync(method);
				ReturnType = GetReturnType(method);
				ParameterType = (method.GetParameters().Length == 0) ? null : method.GetParameters()[0].ParameterType;
			}

			/// <summary>
			/// Invoke the method with the given serialized parameter. Returns a serialized response.
			/// </summary>
			public async Task<string> Invoke(IHealthCheckModule instance, string jsonPayload, IJsonSerializer serializer)
			{
				object[] parameters;
				if (HasParameterType)
				{
					parameters = new object[] { serializer.Deserialize(jsonPayload, ParameterType) };
				}
				else
				{
					parameters = new object[0];
				}

				var result = Method.Invoke(instance, parameters);
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
