using QoDL.Toolkit.Core.Abstractions.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Util.Modules
{
	/// <summary>
	/// Handles loading of modules.
	/// </summary>
	public class ToolkitModuleLoader
	{
		/// <summary>
		/// Load the given module.
		/// </summary>
		public ToolkitLoadedModule Load(IToolkitModule module, ToolkitModuleContext context, string name = null)
		{
			var type = module.GetType();
			var loadedModule = new ToolkitLoadedModule
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
				else if (loadedModule.AccessOptionsType != context.CurrentRequestModuleAccessOptions?.GetType())
				{
					loadedModule.LoadErrors.Add($"Given access options type does not match the module access options type.");
				}
				else if (!EnumUtils.IsTypeEnumFlag(loadedModule.AccessOptionsType))
				{
					loadedModule.LoadErrors.Add($"Access options type should be decorated with [Flags].");
				}

				var getOptionsMethod = type.GetMethods()
					.FirstOrDefault(x => x.Name == nameof(ToolkitModuleBase<StringSplitOptions>.GetFrontendOptionsObject)
										&& x.GetParameters().Length == 1);
				var getConfigMethod = type.GetMethods()
					.FirstOrDefault(x => x.Name == nameof(ToolkitModuleBase<StringSplitOptions>.GetModuleConfig)
										&& x.GetParameters().Length == 1
										&& typeof(IToolkitModuleConfig).IsAssignableFrom(x.ReturnType));
				var validateMethod = type.GetMethods()
					.FirstOrDefault(x => x.Name == nameof(ToolkitModuleBase<StringSplitOptions>.Validate)
										&& x.GetParameters().Length == 0
										&& x.ReturnType == typeof(IEnumerable<string>));

				loadedModule.Config = getConfigMethod.Invoke(module, new object[] { context }) as IToolkitModuleConfig;
				loadedModule.FrontendOptions = getOptionsMethod.Invoke(module, new object[] { context });

				if (loadedModule.Config == null)
				{
					loadedModule.LoadErrors.Add($"GetModuleConfig should return something not null.");
				}
				else
				{
					ValidateConfigValues(loadedModule);
				}

				try
				{
					if (validateMethod.Invoke(module, new object[0]) is IEnumerable<string> issues && issues?.Any() == true)
					{
						loadedModule.LoadErrors.AddRange(issues);
					}
				}
				catch(Exception ex)
				{
					loadedModule.LoadErrors.Add($"Validate() call failed with the error: {ex.Message}");
				}

				if (loadedModule.Config != null && name == null)
				{
					loadedModule.Name = loadedModule?.Config?.Name ?? name;
				}

				loadedModule.InvokableMethods = type.GetMethods()
					.Where(x =>
						x.GetCustomAttributes(true)
						.Any(a => (a is ToolkitModuleMethodAttribute))
					)
					.Select(x => new ToolkitInvokableMethod(x))
					.ToList();

				var dupeMethods = loadedModule.InvokableMethods.GroupBy(x => x.Name).Where(x => x.Count() > 1).ToArray();
				if (dupeMethods.Any())
				{
					loadedModule.LoadErrors.Add($"Contains multiple invokable methods with the same name. Please give them different names. [{string.Join(", ", dupeMethods.Select(x => x.First().Name))}]");
				}

				loadedModule.CustomActions = type.GetMethods()
					.Where(x =>
						x.GetCustomAttributes(true)
						.Any(a => (a is ToolkitModuleActionAttribute))
					)
					.Select(x => new ToolkitInvokableAction(x))
					.ToList();
			}
			catch (Exception ex)
			{
				loadedModule.LoadErrors.Add($"Exception occured on module load: {ex.Message}");
				loadedModule.LoadErrorStacktrace = ex?.ToString();
			}

			return loadedModule;
		}

		private static void ValidateConfigValues(ToolkitLoadedModule loadedModule)
		{
			var requiredPropsNotSet = new List<string>();
			if (loadedModule.Config.DefaultRootRouteSegment == null) requiredPropsNotSet.Add(nameof(IToolkitModuleConfig.DefaultRootRouteSegment));
			if (loadedModule.Config.InitialRoute == null) requiredPropsNotSet.Add(nameof(IToolkitModuleConfig.InitialRoute));
			if (loadedModule.Config.RoutePath == null) requiredPropsNotSet.Add(nameof(IToolkitModuleConfig.RoutePath));

			foreach (var prop in requiredPropsNotSet)
			{
				loadedModule.LoadErrors.Add($"Config property '{prop}' should not be null.");
			}

			if (string.IsNullOrWhiteSpace(loadedModule.Config.ComponentName) && string.IsNullOrWhiteSpace(loadedModule.Config.RawHtml))
            {
				loadedModule.LoadErrors.Add($"Either set config property '{nameof(IToolkitModuleConfig.ComponentName)}' or '{nameof(IToolkitModuleConfig.RawHtml)}'.");

			}
		}

		/// <summary>
		/// Get the generic argument type of the module.
		/// </summary>
		public static Type GetModuleAccessOptionsType(Type moduleType)
		{
			if (!typeof(IToolkitModule).IsAssignableFrom(moduleType))
			{
				throw new ArgumentException($"Can't find module access options type, '{moduleType.Name}' is not assignable to {nameof(IToolkitModule)}.");
			}

			for (int i = 0; i < 100; i++)
			{
				var baseType = moduleType.BaseType;
				if (baseType == null)
				{
					return null;
				}

				if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ToolkitModuleBase<>))
				{
					return baseType.GetGenericArguments()[0];
				}

				moduleType = baseType;
			}
			return null;
		}
	}
}
