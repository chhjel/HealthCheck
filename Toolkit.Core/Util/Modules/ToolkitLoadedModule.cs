using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Util.Modules
{
    /// <summary>
    /// A loaded module.
    /// </summary>
    public class ToolkitLoadedModule
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
		public IToolkitModuleConfig Config { get; set; }

		/// <summary>
		/// Module frontend options object.
		/// </summary>
		public object FrontendOptions { get; set; }

		/// <summary>
		/// List of methods on the module that can be invoked from frontend.
		/// </summary>
		public List<ToolkitInvokableMethod> InvokableMethods { get; set; }

		/// <summary>
		/// List of methods on the module that can be invoked through actions.
		/// </summary>
		public List<ToolkitInvokableAction> CustomActions { get; set; }

		/// <summary>
		/// The module that was loaded.
		/// </summary>
		public IToolkitModule Module { get; set; }

		/// <summary>
		/// All available module categories if any.
		/// </summary>
		public List<string> AllModuleCategories => Module?.AllCategories;

		/// <summary>
		/// All available module ids if any.
		/// </summary>
		public List<TKModuleIdData> AllModuleIds => Module?.AllIds;
	}
}
