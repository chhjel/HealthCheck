using QoDL.Toolkit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Abstractions.Modules
{
    /// <summary>
    /// A custom module visible in the health check.
    /// <para>TModuleAccessOptionsEnum must be a flags enum.</para>
    /// </summary>
    public abstract class ToolkitModuleBase<TModuleAccessOptionsEnum> : IToolkitModule
        where TModuleAccessOptionsEnum : Enum
    {
        /// <inheritdoc />
        public virtual List<string> AllCategories => null;

        /// <inheritdoc />
        public virtual List<TKModuleIdData> AllIds => null;

        /// <summary>
        /// Optional object that will be serialized and used as the options model in frontend for the module pages.
        /// </summary>
        public abstract object GetFrontendOptionsObject(ToolkitModuleContext context);

        /// <summary>
        /// Optional values to be rendered on the page.
        /// </summary>
        public abstract IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context);

        /// <summary>
        /// Optionally validate options etc. Return any issues to be displayed.
        /// </summary>
        public virtual IEnumerable<string> Validate() => Enumerable.Empty<string>();
    }
}
